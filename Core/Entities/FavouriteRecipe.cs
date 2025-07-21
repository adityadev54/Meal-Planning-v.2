using Meal_Planning.Core.Entities;

public class FavoriteRecipe
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    public string RecipeId { get; set; }
    public string RecipeName { get; set; }
    public DateTime DateAdded { get; set; } = DateTime.UtcNow;
    public string Notes { get; set; }
}
