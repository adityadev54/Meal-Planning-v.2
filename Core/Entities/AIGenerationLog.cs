using System.ComponentModel.DataAnnotations;
using Meal_Planning.Core.Entities;

public class AIGenerationLog
{
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    
    public DateTime GenerationDate { get; set; } = DateTime.UtcNow;
    
    [MaxLength(50)]
    public string AIType { get; set; } // "MealPlan", "RecipeSuggestion", etc.
    
    public string PromptUsed { get; set; }
    
    public string ParametersJson { get; set; } // Serialized generation parameters
    
    public string ResponseJson { get; set; } // Serialized AI response
    
    public int TokensUsed { get; set; }
    
    public TimeSpan GenerationTime { get; set; }
    
    public bool WasSuccessful { get; set; }
    
    [MaxLength(500)]
    public string ErrorMessage { get; set; }
}