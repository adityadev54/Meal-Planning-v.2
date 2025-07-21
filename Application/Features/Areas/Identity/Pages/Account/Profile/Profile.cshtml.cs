using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Meal_Planning.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Meal_Planning.Infrastructure.Persistence;
using Meal_Planning.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Meal_Planning.Areas.Identity.Pages.Account.Profile
{
    [Authorize()]
    public class ProfileModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly IDateTimeService _dateTimeService;

        public ProfileModel(
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext dbContext,
            IDateTimeService dateTimeService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
            _dateTimeService = dateTimeService;
            Profile = new ProfileInputModel();
            ChangePassword = new ChangePasswordInputModel();
            Preferences = new PreferencesInputModel();
        }
        
        // Subscription properties
        public string SubscriptionStatus { get; set; } = "Inactive";
        public string SubscriptionPlanName { get; set; } = "";
        public bool TrialActive { get; set; }
        public int TrialDaysLeft { get; set; }

        [BindProperty]
        public ProfileInputModel Profile { get; set; }

        [BindProperty]
        public ChangePasswordInputModel ChangePassword { get; set; }

        public string Version { get; set; } = "v.0.1";

        public class ProfileInputModel
        {
            [Required]
            [Display(Name = "First Name")]
            public string? FirstName { get; set; }

            [Required]
            [Display(Name = "Last Name")]
            public string? LastName { get; set; }

            [Required]
            [EmailAddress]
            public string? Email { get; set; }

            [DataType(DataType.Date)]
            [Display(Name = "Date of Birth")]
            public DateTime? BirthDate { get; set; }

            [Phone]
            [Display(Name = "Phone Number")]
            public string? PhoneNumber { get; set; }

            [Display(Name = "Address")]
            public string? Address { get; set; }
            
            [Display(Name = "City")]
            public string? City { get; set; }
            
            [Display(Name = "Country")]
            public string? Country { get; set; }
            
            [Display(Name = "Zip/Postal Code")]
            public string? ZipCode { get; set; }

            [Display(Name = "Subscribe to newsletter")]
            public bool Newsletter { get; set; }
            
            [Display(Name = "Language Preference")]
            public string? LanguagePreference { get; set; }
            
            [Display(Name = "Preferred Cuisines")]
            public string? PreferredCuisines { get; set; }
            
            [Display(Name = "Cooking Skill Level")]
            public string? CookingSkillLevel { get; set; }
            
            [Display(Name = "Average Cooking Time (minutes)")]
            public int? AverageCookingTime { get; set; }
            
            [Display(Name = "Dietary Restrictions")]
            public string? DietaryRestrictions { get; set; }
            
            [Display(Name = "Food Allergies")]
            public string? Allergies { get; set; }
            
            [Display(Name = "Health Conditions")]
            public string? HealthConditions { get; set; }

            // Non-database info for profile card
            public DateTime? AccountCreated { get; set; }
            public DateTime? LastLogin { get; set; }
            public string? PlanType { get; set; }
        }

        public class ChangePasswordInputModel
        {
            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Current password")]
            public string? OldPassword { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "New password")]
            public string? NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm new password")]
            [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
            public string? ConfirmPassword { get; set; }
        }

        [BindProperty]
        public PreferencesInputModel Preferences { get; set; }

        public class PreferencesInputModel
        {
            [Display(Name = "Font Size")]
            public string FontSize { get; set; } = "normal";

            [Display(Name = "Color Contrast")]
            public string ColorContrast { get; set; } = "default";

            [Display(Name = "Language")]
            public string Language { get; set; } = "en";
        }

        private async Task CheckAndSetSubscriptionStatus(string userId)
        {
            // Get user preferences which contains trial info
            var userPreference = await _dbContext.Preferences
                .FirstOrDefaultAsync(up => up.UserID == userId);

            if (userPreference != null)
            {
                // Check if trial is active
                var trialStartDate = userPreference.TrialStartDate;
                if (trialStartDate.HasValue)
                {
                    DateTime trialEndDate = trialStartDate.Value.AddDays(7); // 7-day trial
                    TrialActive = _dateTimeService.UtcNow < trialEndDate;
                    
                    if (TrialActive)
                    {
                        TrialDaysLeft = Math.Max(0, (int)(trialEndDate - _dateTimeService.UtcNow).TotalDays);
                        SubscriptionStatus = "Trial";
                        SubscriptionPlanName = "Free Trial";
                        return;
                    }
                }
            }
            
            // Check if user has an active subscription
            var subscription = await _dbContext.Subscriptions
                .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);

            if (subscription != null)
            {
                SubscriptionStatus = "Active";
                SubscriptionPlanName = subscription.PlanName;
            }
            else
            {
                SubscriptionStatus = "Inactive";
                SubscriptionPlanName = "";
            }
        }

        [TempData]
        public bool ForcePasswordChange { get; set; }
        
        public async Task<IActionResult> OnGetAsync(bool forcePasswordChange = false)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            // Set force password change flag if user is required to change password
            ForcePasswordChange = forcePasswordChange || user.PasswordChangeRequired;
            
            // Get subscription information
            await CheckAndSetSubscriptionStatus(user.Id);

            // Set PlanType based on subscription status
            string planTypeValue = TrialActive ? "Free Trial" : SubscriptionStatus == "Active" ? SubscriptionPlanName : "Inactive";

            Profile = new ProfileInputModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                BirthDate = user.BirthDate.HasValue ? user.BirthDate.Value : (DateTime?)null,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                City = user.City,
                Country = user.Country,
                ZipCode = user.ZipCode,
                Newsletter = user.Newsletter,
                LanguagePreference = user.LanguagePreference,
                PreferredCuisines = user.PreferredCuisines,
                CookingSkillLevel = user.CookingSkillLevel,
                AverageCookingTime = user.AverageCookingTime,
                DietaryRestrictions = user.DietaryRestrictions,
                Allergies = user.Allergies,
                HealthConditions = user.HealthConditions,
                AccountCreated = user.AccountCreated,
                LastLogin = user.LastLogin,
                PlanType = planTypeValue
            };
            ChangePassword = new ChangePasswordInputModel();
            
            // If forcing password change, add a model state error to inform user
            if (ForcePasswordChange)
            {
                ViewData["PasswordMessage"] = "You must change your password to continue.";
            }
            
            return Page();
        }
        
        public async Task<IActionResult> OnPostChangePasswordAsync()
        {
            // Only validate the ChangePassword model
            ModelState.Clear();
            TryValidateModel(ChangePassword, nameof(ChangePassword));

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            if (!ModelState.IsValid)
            {
                // Get the current subscription status to show in the profile card
                await CheckAndSetSubscriptionStatus(user.Id);
                string currentPlanType = TrialActive ? "Free Trial" : SubscriptionStatus == "Active" ? SubscriptionPlanName : "Inactive";
                
                // Reset form for resubmission
                Profile = new ProfileInputModel
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    BirthDate = user.BirthDate,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address,
                    City = user.City,
                    Country = user.Country,
                    ZipCode = user.ZipCode,
                    Newsletter = user.Newsletter,
                    LanguagePreference = user.LanguagePreference,
                    PreferredCuisines = user.PreferredCuisines,
                    CookingSkillLevel = user.CookingSkillLevel,
                    AverageCookingTime = user.AverageCookingTime,
                    DietaryRestrictions = user.DietaryRestrictions,
                    Allergies = user.Allergies,
                    HealthConditions = user.HealthConditions,
                    AccountCreated = user.AccountCreated,
                    LastLogin = user.LastLogin,
                    PlanType = currentPlanType
                };
                return Page();
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, 
                ChangePassword.OldPassword ?? string.Empty, ChangePassword.NewPassword ?? string.Empty);

            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                
                // Get the current subscription status to show in the profile card
                await CheckAndSetSubscriptionStatus(user.Id);
                string currentPlanType = TrialActive ? "Free Trial" : SubscriptionStatus == "Active" ? SubscriptionPlanName : "Inactive";
                
                // Reset form for resubmission
                Profile = new ProfileInputModel
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    BirthDate = user.BirthDate,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address,
                    City = user.City,
                    Country = user.Country,
                    ZipCode = user.ZipCode,
                    Newsletter = user.Newsletter,
                    LanguagePreference = user.LanguagePreference,
                    PreferredCuisines = user.PreferredCuisines,
                    CookingSkillLevel = user.CookingSkillLevel,
                    AverageCookingTime = user.AverageCookingTime,
                    DietaryRestrictions = user.DietaryRestrictions,
                    Allergies = user.Allergies,
                    HealthConditions = user.HealthConditions,
                    AccountCreated = user.AccountCreated,
                    LastLogin = user.LastLogin,
                    PlanType = currentPlanType
                };
                return Page();
            }

            // Reset the PasswordChangeRequired flag if it was set
            if (user.PasswordChangeRequired)
            {
                user.PasswordChangeRequired = false;
                await _userManager.UpdateAsync(user);
            }

            await _signInManager.RefreshSignInAsync(user);
            ViewData["PasswordMessage"] = "Your password has been changed successfully.";
            
            // Get the current subscription status to show in the profile card
            await CheckAndSetSubscriptionStatus(user.Id);
            string planTypeValue = TrialActive ? "Free Trial" : SubscriptionStatus == "Active" ? SubscriptionPlanName : "Inactive";
            
            // Reset form after successful submission
            ChangePassword = new ChangePasswordInputModel();
            
            // Make sure we have the latest subscription status for display
            await CheckAndSetSubscriptionStatus(user.Id);
            // No new variable declaration
            planTypeValue = TrialActive ? "Free Trial" : SubscriptionStatus == "Active" ? SubscriptionPlanName : "Inactive";
            
            Profile = new ProfileInputModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                BirthDate = user.BirthDate,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                City = user.City,
                Country = user.Country,
                ZipCode = user.ZipCode,
                Newsletter = user.Newsletter,
                LanguagePreference = user.LanguagePreference,
                PreferredCuisines = user.PreferredCuisines,
                CookingSkillLevel = user.CookingSkillLevel,
                AverageCookingTime = user.AverageCookingTime,
                DietaryRestrictions = user.DietaryRestrictions,
                Allergies = user.Allergies,
                HealthConditions = user.HealthConditions,
                AccountCreated = user.AccountCreated,
                LastLogin = user.LastLogin,
                PlanType = planTypeValue
            };
            
            // Redirect to stored return URL if this was a forced password change
            if (TempData["ReturnUrl"] != null && TempData["ReturnUrl"] is string returnUrl && !string.IsNullOrEmpty(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }
            
            return Page();
        }
        
        public async Task<IActionResult> OnPostUpdateProfileAsync()
        {
            // Only validate the Profile model
            ModelState.Clear();
            TryValidateModel(Profile, nameof(Profile));

            if (!ModelState.IsValid)
            {
                ChangePassword = new ChangePasswordInputModel();
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            user.FirstName = Profile.FirstName ?? string.Empty;
            user.LastName = Profile.LastName ?? string.Empty;
            // Fix: Store the full DateTime value instead of just the year
            user.BirthDate = Profile.BirthDate;

            user.PhoneNumber = Profile.PhoneNumber ?? string.Empty;
            user.Address = Profile.Address ?? string.Empty;
            user.City = Profile.City ?? string.Empty;
            user.Country = Profile.Country ?? string.Empty;
            user.ZipCode = Profile.ZipCode ?? string.Empty;
            user.Newsletter = Profile.Newsletter;
            user.LanguagePreference = Profile.LanguagePreference ?? "en-US";
            user.PreferredCuisines = Profile.PreferredCuisines ?? string.Empty;
            user.CookingSkillLevel = Profile.CookingSkillLevel ?? string.Empty;
            user.AverageCookingTime = Profile.AverageCookingTime;
            user.DietaryRestrictions = Profile.DietaryRestrictions ?? string.Empty;
            user.Allergies = Profile.Allergies ?? string.Empty;
            user.HealthConditions = Profile.HealthConditions ?? string.Empty;

            // Only update email if changed and valid
            if (user.Email != Profile.Email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, Profile.Email);
                if (!setEmailResult.Succeeded)
                {
                    foreach (var error in setEmailResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);
                    ChangePassword = new ChangePasswordInputModel();
                    return Page();
                }
            }


            // This is important: update the user in the database
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                ChangePassword = new ChangePasswordInputModel();
                return Page();
            }

            await _signInManager.RefreshSignInAsync(user);
            ViewData["Message"] = "Profile updated.";
            ChangePassword = new ChangePasswordInputModel();
            return Page();
        }
    }
}