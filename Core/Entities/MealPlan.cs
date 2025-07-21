using System.ComponentModel.DataAnnotations;

namespace Meal_Planning.Core.Entities
{
    public class MealPlan
    {
        [Key]
        public int PlanID { get; set; }
        public string? UserID { get; set; }

        // JSON Stored Meal Plan
        public string? PlanData { get; set; }
        
        // Recipe instructions with step-by-step guides
        public string? RecipeInstructions { get; set; }
        
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
        public bool IsFavorite { get; set; } = false;
        public string Notes { get; set; } = string.Empty;
        public string PlanJson { get; set; } = string.Empty;
        public string ParameterJson { get; set; } = string.Empty;

        public ApplicationUser? User { get; set; }
    }
}
