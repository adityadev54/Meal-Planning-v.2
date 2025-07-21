using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Meal_Planning.Core.Entities;
using Meal_Planning.Infrastructure.Persistence;
using Meal_Planning.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Meal_Planning.Areas.Identity.Pages.Account.Subscriptions
{
    [Authorize]
    public class ManageSubscriptionModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly IDateTimeService _dateTimeService;

        public ManageSubscriptionModel(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext dbContext,
            IDateTimeService dateTimeService)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _dateTimeService = dateTimeService;
        }

        public bool TrialActive { get; set; }
        public int TrialDaysLeft { get; set; }
        public DateTime TrialEndDate { get; set; }
        
        public bool HasActiveSubscription { get; set; }
        public string SubscriptionPlanName { get; set; } = "";
        public decimal SubscriptionAmount { get; set; }
        public DateTime? NextBillingDate { get; set; }
        
        public List<Subscription> PastSubscriptions { get; set; } = new List<Subscription>();

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadSubscriptionData(user.Id);
            
            return Page();
        }
        
        public async Task<IActionResult> OnPostCancelSubscriptionAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var activeSubscription = await _dbContext.Subscriptions
                .FirstOrDefaultAsync(s => s.UserId == user.Id && s.Status == "Active");

            if (activeSubscription == null)
            {
                ViewData["ErrorMessage"] = "No active subscription found.";
                await LoadSubscriptionData(user.Id);
                return Page();
            }

            // Mark subscription as cancelled but keep it active until end of billing period
            activeSubscription.Status = "Cancelled";
            
            // Keep IsActive true until expiry date since user paid for the full period
            // IsActive will be updated by the background service when ExpiresAt is reached
            
            await _dbContext.SaveChangesAsync();
            
            ViewData["SuccessMessage"] = "Your subscription has been cancelled. You will have access to premium features until the end of your current billing period.";
            
            await LoadSubscriptionData(user.Id);
            return Page();
        }
        
        private async Task LoadSubscriptionData(string userId)
        {
            // Check trial status
            var userPreference = await _dbContext.Preferences
                .FirstOrDefaultAsync(up => up.UserID == userId);

            if (userPreference?.TrialStartDate != null)
            {
                TrialEndDate = userPreference.TrialStartDate.Value.AddDays(7);
                TrialActive = _dateTimeService.UtcNow < TrialEndDate;
                
                if (TrialActive)
                {
                    TrialDaysLeft = Math.Max(0, (int)(TrialEndDate - _dateTimeService.UtcNow).TotalDays);
                }
            }

            // Get current active subscription
            var activeSubscription = await _dbContext.Subscriptions
                .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == "Active");

            HasActiveSubscription = activeSubscription != null;
            
            if (HasActiveSubscription && activeSubscription != null)
            {
                SubscriptionPlanName = activeSubscription.PlanName;
                SubscriptionAmount = activeSubscription.Amount;
                NextBillingDate = activeSubscription.ExpiresAt;
            }

            // Get past subscriptions
            PastSubscriptions = await _dbContext.Subscriptions
                .Where(s => s.UserId == userId && s.Status != "Active")
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }
    }
}
