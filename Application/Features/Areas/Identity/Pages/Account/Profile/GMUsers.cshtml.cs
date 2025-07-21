using Meal_Planning.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace Meal_Planning.Application.Features.Areas.Identity.Pages.Account.Profile
{
    [Authorize(Roles = "Admin")]
    public class GMUsersModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public GMUsersModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public IList<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

        public async Task OnGetAsync()
        {
            Users = _userManager.Users.ToList();
        }
    }
}