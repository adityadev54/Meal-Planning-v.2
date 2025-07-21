using System;

namespace Meal_Planning.Core.Entities
{
    public class GroceryItem
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int PlanId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public bool IsCompleted { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public string Category { get; set; } = "Other";
        
        // Navigation properties
        public virtual ApplicationUser? User { get; set; }
        public virtual MealPlan? MealPlan { get; set; }
    }
}
