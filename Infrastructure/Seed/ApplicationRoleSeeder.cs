using Meal_Planning.Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace Meal_Planning.Infrastructure.Seed
{
    public static class ApplicationRoleSeeder
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            string[] roleNames = { "Admin", "Member", "Developer" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }
    }
}