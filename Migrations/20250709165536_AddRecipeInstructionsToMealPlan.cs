using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Meal_Planning.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipeInstructionsToMealPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RecipeInstructions",
                table: "MealPlanResults",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecipeInstructions",
                table: "MealPlanResults");
        }
    }
}
