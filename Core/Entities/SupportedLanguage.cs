using System;
using System.ComponentModel.DataAnnotations;

namespace Meal_Planning.Core.Entities
{
    public class SupportedLanguage
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(10)]
        public string LanguageCode { get; set; } // e.g., "en-US", "es-ES"
        
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } // e.g., "English (US)", "Spanish (Spain)"
        
        [Required]
        [MaxLength(50)]
        public string NativeName { get; set; } // e.g., "English (US)", "Español (España)"
        
        public bool IsActive { get; set; } = true;
        
        public int DisplayOrder { get; set; }
        
        public DateTime DateAdded { get; set; }
    }
}
