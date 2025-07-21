using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Meal_Planning.Migrations
{
    public partial class AddSupportedLanguages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SupportedLanguages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LanguageCode = table.Column<string>(maxLength: 10, nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: false),
                    NativeName = table.Column<string>(maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(nullable: false, defaultValue: true),
                    DisplayOrder = table.Column<int>(nullable: false),
                    DateAdded = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportedLanguages", x => x.Id);
                });

            // Add index for quick lookups by language code
            migrationBuilder.CreateIndex(
                name: "IX_SupportedLanguages_LanguageCode",
                table: "SupportedLanguages",
                column: "LanguageCode",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupportedLanguages");
        }
    }
}
