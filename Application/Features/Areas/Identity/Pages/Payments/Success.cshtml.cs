// Application/Features/Areas/Identity/Pages/Payments/SuccessModel.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Meal_Planning.Application.Features.Areas.Identity.Pages.Payments
{
    public class SuccessModel : PageModel
    {
        public string SessionId { get; set; }
        public string PlanName { get; set; }
        public decimal Amount { get; set; }

        public IActionResult OnGet(string sessionId, string planName, decimal amount)
        {
            if (string.IsNullOrEmpty(sessionId) || string.IsNullOrEmpty(planName))
            {
                return RedirectToPage("/Index");
            }

            SessionId = sessionId;
            PlanName = planName;
            Amount = amount;

            return Page();
        }
    }
}