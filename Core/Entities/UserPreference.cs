using System.ComponentModel.DataAnnotations;

namespace Meal_Planning.Core.Entities
{
    public class UserPreference
    {
        [Key]
        public int PrefID { get; set; }
        public string? UserID { get; set; }
        public string Likes { get; set; } = string.Empty;
        public string Dislikes { get; set; } = string.Empty;
        public string Allergies { get; set; } = string.Empty;
        public string DietaryRestriction { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
        public int MealPlanGenerations { get; set; } = 0;
        public DateTime? TrialStartDate { get; set; }
    }
}
