using System;
using System.ComponentModel.DataAnnotations;

namespace Meal_Planning.Core.Entities
{
    public class VisitorLog
    {
        [Key]
        public int Id { get; set; }
        
        public string IpAddress { get; set; } = string.Empty;
        
        public string Path { get; set; } = string.Empty;
        
        public string Method { get; set; } = string.Empty;
        
        public string UserAgent { get; set; } = string.Empty;
        
        public string Referrer { get; set; } = string.Empty;
        
        public int StatusCode { get; set; }
        
        public double ResponseTimeMs { get; set; }
        
        public DateTime VisitedAt { get; set; } = DateTime.UtcNow;
        
        public string? CountryCode { get; set; }
        
        public string? City { get; set; }
        
        // If you're tracking authenticated users
        public string? UserId { get; set; }
    }
}
