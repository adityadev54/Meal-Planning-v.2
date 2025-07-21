using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Meal_Planning.Core.Entities;
using Meal_Planning.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;

namespace Meal_Planning.Infrastructure.Middleware
{
    public class VisitorTrackingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<VisitorTrackingMiddleware> _logger;

        public VisitorTrackingMiddleware(RequestDelegate next, ILogger<VisitorTrackingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var startTime = DateTime.UtcNow;
            
            // Get IP address
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var userAgent = context.Request.Headers["User-Agent"].ToString();
            var path = context.Request.Path;
            var referrer = context.Request.Headers["Referer"].ToString();
            var method = context.Request.Method;

            // Log the visitor information
            _logger.LogInformation(
                "Visitor: IP={IpAddress}, Path={Path}, Method={Method}, UserAgent={UserAgent}, Referrer={Referrer}",
                ipAddress, path, method, userAgent, referrer
            );

            // Call the next middleware in the pipeline
            await _next(context);

            // Calculate response time
            var responseTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
            
            // Log response information
            _logger.LogInformation(
                "Response: IP={IpAddress}, Path={Path}, StatusCode={StatusCode}, Duration={Duration}ms",
                ipAddress, path, context.Response.StatusCode, responseTime
            );
            
            // Don't log static files or specific paths
            if (path.StartsWithSegments("/assets") || 
                path.StartsWithSegments("/css") || 
                path.StartsWithSegments("/js") ||
                path.StartsWithSegments("/favicon.ico"))
            {
                return;
            }
            
            try
            {
                // Get the user ID if authenticated
                string? userId = null;
                if (context.User?.Identity?.IsAuthenticated == true)
                {
                    var userManager = context.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
                    var user = await userManager.GetUserAsync(context.User);
                    userId = user?.Id;
                }
                
                // Create visitor log entry
                var visitorLog = new VisitorLog
                {
                    IpAddress = ipAddress,
                    Path = path,
                    Method = method,
                    UserAgent = userAgent,
                    Referrer = referrer,
                    StatusCode = context.Response.StatusCode,
                    ResponseTimeMs = responseTime,
                    VisitedAt = DateTime.UtcNow,
                    UserId = userId
                };
                
                // Get the DbContext from DI
                using (var scope = context.RequestServices.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    dbContext.VisitorLogs.Add(visitorLog);
                    await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't stop the request
                _logger.LogError(ex, "Error saving visitor log");
            }
        }
    }

    // Extension method to make it easier to add the middleware to the pipeline
    public static class VisitorTrackingMiddlewareExtensions
    {
        public static IApplicationBuilder UseVisitorTracking(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<VisitorTrackingMiddleware>();
        }
    }
}
