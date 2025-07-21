using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Meal_Planning.Infrastructure.Middleware
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SecurityHeadersMiddleware> _logger;

        public SecurityHeadersMiddleware(RequestDelegate next, ILogger<SecurityHeadersMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Add security headers to all responses
            AddSecurityHeaders(context);

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred during request processing");
                throw;
            }
        }

        private void AddSecurityHeaders(HttpContext context)
        {
            // Content Security Policy
            context.Response.Headers["Content-Security-Policy"] = 
                "default-src 'self'; " +
                "script-src 'self' https://cdn.jsdelivr.net https://code.jquery.com 'unsafe-inline'; " +
                "style-src 'self' https://cdn.jsdelivr.net 'unsafe-inline'; " +
                "img-src 'self' data: https://images.unsplash.com; " +
                "font-src 'self' https://cdn.jsdelivr.net; " +
                "connect-src 'self'; " +
                "frame-src 'self'; " +
                "object-src 'none'";

            // Prevents browser from MIME-sniffing a response away from the declared content type
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            
            // Prevents clickjacking by instructing the browser not to display the page in a frame
            context.Response.Headers["X-Frame-Options"] = "DENY";
            
            // XSS protection
            context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
            
            // Referrer policy
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            
            // Tells browsers that this site should only be accessed using HTTPS
            // Enable only in production with HTTPS
            // context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
            
            // Permissions Policy (formerly Feature-Policy)
            context.Response.Headers["Permissions-Policy"] = 
                "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()";
        }
    }
}
