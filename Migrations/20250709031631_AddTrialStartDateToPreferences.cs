using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Meal_Planning.Migrations
{
    /// <inheritdoc />
    public partial class AddTrialStartDateToPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TrialStartDate",
                table: "Preferences",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TrialStartDate",
                table: "Preferences");
        }
    }
}
