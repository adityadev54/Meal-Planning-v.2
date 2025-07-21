using Meal_Planning.Core.Entities;

public class UserActivityLog
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    public string ActivityType { get; set; } // "Login", "MealPlanGenerated", etc.
    public string Description { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string IpAddress { get; set; }
}