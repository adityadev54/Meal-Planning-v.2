using Meal_Planning.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Meal_Planning.Application.Features.Areas.Identity.Pages.Account.Denial
{
    public class AccessDeniedModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AccessDeniedModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public string FirstName { get; set; } = "Unknown";
        public IList<string> Roles { get; set; } = new List<string>();

        public async Task OnGetAsync()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    FirstName = user.FirstName ?? user.UserName ?? "Unknown";
                    Roles = await _userManager.GetRolesAsync(user);
                }
            }
        }
    }
}