// Application/Features/Areas/Identity/Pages/Payments/PaymentModel.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Meal_Planning.Infrastructure.Services;
using Meal_Planning.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Meal_Planning.Core.Entities;
using System.Threading.Tasks;
using System.Linq;

namespace Meal_Planning.Application.Features.Areas.Identity.Pages.Payments
{
    public class PaymentModel : PageModel
    {
        private readonly IPaymentService _paymentService;
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public string PlanId { get; set; }
        public string PlanName { get; set; }
        public decimal Amount { get; set; }
        public string PaymentIntentId { get; set; }
        public UserDetailsDto UserDetails { get; set; }

        public PaymentModel(IPaymentService paymentService, 
                         ApplicationDbContext dbContext, 
                         UserManager<ApplicationUser> userManager)
        {
            _paymentService = paymentService;
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync(string planId, decimal amount, string planName)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Auths/Login", new { area = "Identity", returnUrl = Url.Page("/Payments/MealPlanPayments") });
            }

            // Get user details from database
            UserDetails = await GetUserDetails(user.Id);

            var plans = new List<Plan>
            {
                new Plan { Id = "basic", Name = "Basic Plan", Price = 0 },
                new Plan { Id = "mealplan", Name = "Meal Plan Only", Price = 5.00m },
                new Plan { Id = "premium", Name = "Premium Plan", Price = 0 }
            };
            var plan = plans.FirstOrDefault(p => p.Id.Equals(planId, StringComparison.OrdinalIgnoreCase));
            if (plan == null || plan.Price != amount || plan.Name != planName)
            {
                ViewData["ErrorMessage"] = "Invalid plan selected.";
                return RedirectToPage("MealPlanPayments");
            }

            var paymentIntent = await _paymentService.CreatePaymentIntentAsync(amount, "usd", planId, planName);
            PlanId = planId;
            PlanName = planName;
            Amount = amount;
            PaymentIntentId = paymentIntent.Id;

            return Page();
        }

        public async Task<IActionResult> OnPostConfirmPaymentAsync(
            string planId, 
            string billing, 
            string paymentIntentId, 
            string cardNumber, 
            string expiry, 
            string cvc,
            string cardName,
            bool saveCard = false)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Auths/Login", new { area = "Identity", returnUrl = Url.Page("/Payments/MealPlanPayments") });
            }

            // Get user details again in case we need to redisplay the page
            UserDetails = await GetUserDetails(user.Id);

            var plans = new List<Plan>
            {
                new Plan { Id = "basic", Name = "Basic Plan", Price = 0 },
                new Plan { Id = "mealplan", Name = "Meal Plan Only", Price = 5.00m },
                new Plan { Id = "premium", Name = "Premium Plan", Price = 0 }
            };
            var selectedPlan = plans.FirstOrDefault(p => p.Id.Equals(planId, StringComparison.OrdinalIgnoreCase));
            if (selectedPlan == null)
            {
                ViewData["ErrorMessage"] = "Invalid plan selected.";
                return Page();
            }

            try
            {
                // Enhanced server-side validation
                if (string.IsNullOrEmpty(cardName))
                {
                    ViewData["ErrorMessage"] = "Cardholder name is required.";
                    return Page();
                }

                if (!System.Text.RegularExpressions.Regex.IsMatch(cardNumber?.Replace(" ", "") ?? "", @"^\d{16}$"))
                {
                    ViewData["ErrorMessage"] = "Invalid card number. Must be 16 digits.";
                    return Page();
                }

                if (!System.Text.RegularExpressions.Regex.IsMatch(expiry ?? "", @"^\d{2}/\d{2}$"))
                {
                    ViewData["ErrorMessage"] = "Invalid expiry date format. Use MM/YY.";
                    return Page();
                }

                if (!System.Text.RegularExpressions.Regex.IsMatch(cvc ?? "", @"^\d{3,4}$"))
                {
                    ViewData["ErrorMessage"] = "Invalid CVC. Must be 3 or 4 digits.";
                    return Page();
                }

                // Process payment
                await _paymentService.ConfirmPaymentAsync(paymentIntentId);

                // Optionally save card details if user selected to
                // if (saveCard)
                // {
                //     var paymentMethod = new PaymentMethod
                //     {
                //         UserId = user.Id,
                //         CardLastFour = cardNumber.Substring(cardNumber.Length - 4),
                //         CardBrand = GetCardBrand(cardNumber),
                //         ExpiryDate = expiry,
                //         IsDefault = true,
                //         CreatedAt = DateTime.UtcNow
                //     };
                //     _dbContext.PaymentMethods.Add(paymentMethod);
                // }

                // Create subscription
                var subscription = new Subscription
                {
                    UserId = user.Id,
                    PlanId = selectedPlan.Id,
                    PlanName = selectedPlan.Name,
                    Amount = selectedPlan.Price,
                    PaymentIntentId = paymentIntentId,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMonths(1)
                };

                _dbContext.Subscriptions.Add(subscription);
                await _dbContext.SaveChangesAsync();

                return RedirectToPage("Success", new { sessionId = paymentIntentId });
            }
            catch (Exception e)
            {
                ViewData["ErrorMessage"] = $"Payment failed: {e.Message}";
                return Page();
            }
        }

        private async Task<UserDetailsDto> GetUserDetails(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            // Assuming your ApplicationUser has these properties
            return new UserDetailsDto
            {
                FullName = $"{user.FirstName} {user.LastName}",
                Email = user.Email,
                AddressLine1 = user.Address,
                City = user.City,
                PostalCode = user.ZipCode,
                Country = user.Country
            };
        }

        private string GetCardBrand(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber)) return "Unknown";
            
            cardNumber = cardNumber.Replace(" ", "");
            
            if (cardNumber.StartsWith("4")) return "Visa";
            if (cardNumber.StartsWith("5")) return "Mastercard";
            if (cardNumber.StartsWith("34") || cardNumber.StartsWith("37")) return "American Express";
            if (cardNumber.StartsWith("6")) return "Discover";
            
            return "Unknown";
        }

        private class Plan
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
        }
    }

    public class UserDetailsDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
    }
}