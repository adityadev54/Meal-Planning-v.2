using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Meal_Planning.Infrastructure.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private static readonly Dictionary<string, ClientStatistics> _clients = new Dictionary<string, ClientStatistics>();
        private readonly int _maxRequestsPerMinute;

        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _maxRequestsPerMinute = 60; // Can be loaded from configuration
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientIp = GetClientIpAddress(context);
            var endpoint = context.Request.Path.Value?.ToLowerInvariant() ?? "";

            // Skip rate limiting for static files
            if (endpoint.StartsWith("/css/") || 
                endpoint.StartsWith("/js/") || 
                endpoint.StartsWith("/img/") ||
                endpoint.StartsWith("/lib/"))
            {
                await _next(context);
                return;
            }

            ClientStatistics clientStats;
            var clientKey = $"{clientIp}";

            lock (_clients)
            {
                if (!_clients.TryGetValue(clientKey, out clientStats))
                {
                    clientStats = new ClientStatistics();
                    _clients[clientKey] = clientStats;
                }
            }

            // Clean up old requests
            clientStats.RemoveOldRequests();

            if (clientStats.RequestCount >= _maxRequestsPerMinute)
            {
                _logger.LogWarning("Rate limit exceeded for IP: {ClientIp}", clientIp);
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.Headers["Retry-After"] = "60";
                await context.Response.WriteAsync("Too many requests. Please try again later.");
                return;
            }

            // Register this request
            clientStats.RegisterRequest();

            // Clean up clients dictionary periodically
            if (new Random().Next(100) == 0) // 1% chance to clean up
            {
                CleanupOldClients();
            }

            await _next(context);
        }

        private string GetClientIpAddress(HttpContext context)
        {
            // Try to get client IP address from X-Forwarded-For header (used by reverse proxies)
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }
            
            // Fallback to connection remote IP address
            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        private void CleanupOldClients()
        {
            try
            {
                var cutoff = DateTime.UtcNow.AddMinutes(-5);
                var clientsToRemove = new List<string>();

                lock (_clients)
                {
                    foreach (var kvp in _clients)
                    {
                        if (kvp.Value.LastRequestTime < cutoff)
                        {
                            clientsToRemove.Add(kvp.Key);
                        }
                    }

                    foreach (var key in clientsToRemove)
                    {
                        _clients.Remove(key);
                    }
                }

                _logger.LogInformation("Cleaned up {Count} inactive clients", clientsToRemove.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up old clients");
            }
        }

        private class ClientStatistics
        {
            private readonly List<DateTime> _requests = new List<DateTime>();
            private readonly object _lock = new object();
            public DateTime LastRequestTime { get; private set; } = DateTime.UtcNow;

            public int RequestCount
            {
                get
                {
                    lock (_lock)
                    {
                        return _requests.Count;
                    }
                }
            }

            public void RegisterRequest()
            {
                lock (_lock)
                {
                    _requests.Add(DateTime.UtcNow);
                    LastRequestTime = DateTime.UtcNow;
                }
            }

            public void RemoveOldRequests()
            {
                lock (_lock)
                {
                    var cutoff = DateTime.UtcNow.AddMinutes(-1);
                    _requests.RemoveAll(r => r < cutoff);
                }
            }
        }
    }
}
