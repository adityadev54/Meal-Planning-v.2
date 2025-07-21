using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Meal_Planning.Migrations
{
    /// <inheritdoc />
    public partial class SeedDefaultLanguagesData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert default languages
            migrationBuilder.InsertData(
                table: "SupportedLanguages",
                columns: new[] { "LanguageCode", "Name", "NativeName", "IsActive", "DisplayOrder", "DateAdded" },
                values: new object[,]
                {
                    { "en-US", "English (US)", "English (US)", true, 1, DateTime.UtcNow },
                    { "es-ES", "Spanish (Spain)", "Español (España)", true, 2, DateTime.UtcNow },
                    { "fr-FR", "French (France)", "Français (France)", true, 3, DateTime.UtcNow },
                    { "de-DE", "German (Germany)", "Deutsch (Deutschland)", true, 4, DateTime.UtcNow }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SupportedLanguages",
                keyColumn: "LanguageCode",
                keyValues: new object[] { "en-US", "es-ES", "fr-FR", "de-DE" });
        }
    }
}
