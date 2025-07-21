using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Meal_Planning.Migrations
{
    /// <inheritdoc />
    public partial class AddMealPlanGenerationsToUserPreference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MealPlanGenerations",
                table: "Preferences",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MealPlanGenerations",
                table: "Preferences");
        }
    }
}
