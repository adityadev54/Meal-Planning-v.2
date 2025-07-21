using System;
using System.ComponentModel.DataAnnotations;

namespace Meal_Planning.Core.Entities
{
    public class Feedback
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string FeedbackType { get; set; } = string.Empty; // Feature, Bug, Suggestion, Other

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [StringLength(1000, MinimumLength = 10)]
        public string Message { get; set; } = string.Empty;

        // Optional: store the file path or name of the uploaded attachment
        [StringLength(260)]
        public string? AttachmentPath { get; set; }

        // Optional: enable if you want to support ratings(1-5)
        [Range(1, 5)]
        public int? Rating { get; set; }

        public string? AttachmentFileName { get; set; }
        public byte[]? AttachmentData { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }
}