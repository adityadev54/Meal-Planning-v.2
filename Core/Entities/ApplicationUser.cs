using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Meal_Planning.Core.Entities;

public class ApplicationUser : IdentityUser
{
    // Basic User Information
    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; }

    [Required]
    [MaxLength(50)]
    public string LastName { get; set; }

    [PersonalData]
    public DateTime? BirthDate { get; set; }

    // Contact Information
    [MaxLength(100)]
    public string Address { get; set; }

    [MaxLength(20)]
    public string ZipCode { get; set; }

    [MaxLength(50)]
    public string? City { get; set; }

    [MaxLength(50)]
    public string? Country { get; set; }

    // Preferences
    public bool Newsletter { get; set; }
    public bool? DarkModeEnabled { get; set; }
    public string? LanguagePreference { get; set; } = "en-US";

    // Health Profile
    public string? DietaryRestrictions { get; set; }
    public string? Allergies { get; set; }
    public string? HealthConditions { get; set; }
    public string? FitnessGoals { get; set; }

    // Meal Planning Specific
    public int? DefaultMealsPerDay { get; set; } = 3;
    public string? PreferredCuisines { get; set; }
    public string? CookingSkillLevel { get; set; } // Beginner, Intermediate, Advanced
    public int? AverageCookingTime { get; set; } // in minutes

    // Tracking
    public DateTime AccountCreated { get; set; } = DateTime.UtcNow;
    public DateTime LastLogin { get; set; }
    public int MealPlansGenerated { get; set; }
    
    // Security
    public bool PasswordChangeRequired { get; set; } = false;

    // Ollama AI Settings
    public bool UseOllamaAI { get; set; } = false;
    
    [MaxLength(50)]
    public string PreferredAIModel { get; set; } = "llama3";
    
    public float AITemperature { get; set; } = 0.7f;
    
    public int AIMaxTokens { get; set; } = 1024;
    
    [MaxLength(500)]
    public string AICustomInstructions { get; set; } = string.Empty;

    // Navigation Properties
    public ICollection<UserPreference> Preferences { get; set; } = new List<UserPreference>();
    public ICollection<MealPlan> MealPlanResults { get; set; } = new List<MealPlan>();
    public ICollection<UserActivityLog> ActivityLogs { get; set; } = new List<UserActivityLog>();
    public ICollection<FavoriteRecipe> FavoriteRecipes { get; set; } = new List<FavoriteRecipe>();
    // Navigation property for AI-generated content
    public ICollection<AIGenerationLog> AIGenerationLogs { get; set; } = new List<AIGenerationLog>();

    // Helper method for AI settings
    [NotMapped]
    public Dictionary<string, object> OllamaSettings => new()
    {
        { "UseOllamaAI", UseOllamaAI },
        { "PreferredAIModel", PreferredAIModel },
        { "Temperature", AITemperature },
        { "MaxTokens", AIMaxTokens },
        { "CustomInstructions", AICustomInstructions }
    };

    // Helper Properties
    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";

    [NotMapped]
    public string Initials => $"{FirstName?[0]}{LastName?[0]}".ToUpper();
}