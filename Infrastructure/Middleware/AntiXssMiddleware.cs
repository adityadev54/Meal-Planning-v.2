using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Meal_Planning.Infrastructure.Middleware
{
    public class AntiXssMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AntiXssMiddleware> _logger;

        public AntiXssMiddleware(RequestDelegate next, ILogger<AntiXssMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check XSS in URL
            if (!string.IsNullOrEmpty(context.Request.Path.Value))
            {
                var url = context.Request.Path.Value;

                if (IsSuspiciousPayload(url))
                {
                    _logger.LogWarning("XSS attempt detected in URL: {Url}", url);
                    context.Response.Clear();
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("Bad request");
                    return;
                }
            }

            // Check XSS in query string
            if (context.Request.QueryString.HasValue)
            {
                var queryString = context.Request.QueryString.Value;

                if (IsSuspiciousPayload(queryString))
                {
                    _logger.LogWarning("XSS attempt detected in query string: {QueryString}", queryString);
                    context.Response.Clear();
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("Bad request");
                    return;
                }
            }

            // Check form data if it's a POST request
            if (context.Request.Method == "POST" && context.Request.HasFormContentType)
            {
                var form = await context.Request.ReadFormAsync();
                foreach (var key in form.Keys)
                {
                    var value = form[key].ToString();
                    if (IsSuspiciousPayload(value))
                    {
                        _logger.LogWarning("XSS attempt detected in form field {Key}: {Value}", key, value);
                        context.Response.Clear();
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsync("Bad request");
                        return;
                    }
                }
            }

            await _next(context);
        }

        private bool IsSuspiciousPayload(string payload)
        {
            if (string.IsNullOrEmpty(payload))
                return false;

            // Check for common XSS patterns
            payload = payload.ToLower();
            
            // Detect script tags
            if (payload.Contains("<script") || payload.Contains("</script>"))
                return true;
            
            // Detect javascript: protocol
            if (payload.Contains("javascript:"))
                return true;
            
            // Detect event handlers
            if (payload.Contains("onload=") || payload.Contains("onerror=") || 
                payload.Contains("onclick=") || payload.Contains("onmouseover="))
                return true;
            
            // Detect data: URI
            if (payload.Contains("data:text/html"))
                return true;
            
            // Detect dangerous HTML tags
            if (payload.Contains("<iframe") || payload.Contains("<object") || 
                payload.Contains("<embed") || payload.Contains("<img") && payload.Contains("onerror"))
                return true;
            
            // Detect eval() and other dangerous JS functions
            if (payload.Contains("eval(") || payload.Contains("document.cookie") || 
                payload.Contains("document.domain") || payload.Contains("document.write"))
                return true;
            
            return false;
        }
    }
}
