// Application/Features/Areas/Identity/Pages/Payments/MealPlanPaymentsModel.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Meal_Planning.Infrastructure.Services;
using Meal_Planning.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Meal_Planning.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Meal_Planning.Application.Features.Areas.Identity.Pages.Payments
{
    public class MealPlanPaymentsModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IPaymentService _paymentService;
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public bool IsDemoMode { get; set; }
        public List<Plan> Plans { get; set; } = new List<Plan>();

        public MealPlanPaymentsModel(
            IConfiguration configuration,
            IPaymentService paymentService,
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager)
        {
            _configuration = configuration;
            _paymentService = paymentService;
            _dbContext = dbContext;
            _userManager = userManager;

            IsDemoMode = _configuration.GetValue<bool>("Payment:DemoMode", true);
            InitializePlans();
        }

        private void InitializePlans()
        {
            Plans = new List<Plan>
            {
                new Plan
                {
                    Id = "basic",
                    Name = "Basic Plan",
                    Price = _configuration.GetValue<decimal>("Plans:BasicPrice", 0),
                    Description = "Access to recorded exercises, recipes, and nutrition guides.",
                    Features = new List<PlanFeature>
                    {
                        new PlanFeature { Text = "100+ recorded exercise videos", IsIncluded = true },
                        new PlanFeature { Text = "200+ healthy recipes", IsIncluded = true },
                        new PlanFeature { Text = "Nutrition guides & tips", IsIncluded = true },
                        new PlanFeature { Text = "No live exercises", IsIncluded = false },
                        new PlanFeature { Text = "No personalized meal plans", IsIncluded = false }
                    }
                },
                new Plan
                {
                    Id = "mealplan",
                    Name = "Meal Plan Only",
                    Price = _configuration.GetValue<decimal>("Plans:MealPlanPrice", 5.00m),
                    Description = "Personalized meal plans crafted by dietitians.",
                    IsPopular = true,
                    Features = new List<PlanFeature>
                    {
                        new PlanFeature { Text = "Weekly personalized meal plans", IsIncluded = true },
                        new PlanFeature { Text = "Grocery lists & prep guides", IsIncluded = true },
                        new PlanFeature { Text = "200+ healthy recipes", IsIncluded = true },
                        new PlanFeature { Text = "No live or recorded exercises", IsIncluded = false },
                        new PlanFeature { Text = "No nutrition guides", IsIncluded = false }
                    }
                },
                new Plan
                {
                    Id = "premium",
                    Name = "Premium Plan",
                    Price = _configuration.GetValue<decimal>("Plans:PremiumPrice", 0),
                    Description = "Live exercises and personalized meal plans for a complete fitness experience.",
                    Features = new List<PlanFeature>
                    {
                        new PlanFeature { Text = "Weekly live exercise classes", IsIncluded = true },
                        new PlanFeature { Text = "Weekly personalized meal plans", IsIncluded = true },
                        new PlanFeature { Text = "100+ recorded exercise videos", IsIncluded = true },
                        new PlanFeature { Text = "200+ healthy recipes", IsIncluded = true },
                        new PlanFeature { Text = "Nutrition guides & tips", IsIncluded = true }
                    }
                }
            };
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostSubscribeAsync(string plan, string billing, string planName, decimal amount)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Auths/Login", new { area = "Identity", returnUrl = Url.Page("/Payments/MealPlanPayments") });
            }

            var selectedPlan = Plans.FirstOrDefault(p => p.Id.Equals(plan, StringComparison.OrdinalIgnoreCase));
            if (selectedPlan == null || selectedPlan.Name != planName || selectedPlan.Price != amount)
            {
                ViewData["ErrorMessage"] = "Invalid plan selected.";
                return Page();
            }

            if (selectedPlan.Price == 0)
            {
                var subscription = new Subscription
                {
                    UserId = user.Id,
                    PlanId = selectedPlan.Id,
                    PlanName = selectedPlan.Name,
                    Amount = selectedPlan.Price,
                    PaymentIntentId = $"mock_{Guid.NewGuid()}",
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMonths(1)
                };

                _dbContext.Subscriptions.Add(subscription);
                await _dbContext.SaveChangesAsync();

                ViewData["Message"] = $"Successfully subscribed to {selectedPlan.Name}!";
                return RedirectToPage("Success", new { sessionId = subscription.PaymentIntentId });
            }
            else
            {
                return RedirectToPage("Payment", new { planId = selectedPlan.Id, planName = selectedPlan.Name, amount = selectedPlan.Price });
            }
        }
    }

    public class Plan
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public bool IsPopular { get; set; }
        public List<PlanFeature> Features { get; set; }
    }

    public class PlanFeature
    {
        public string Text { get; set; }
        public bool IsIncluded { get; set; }
    }
}