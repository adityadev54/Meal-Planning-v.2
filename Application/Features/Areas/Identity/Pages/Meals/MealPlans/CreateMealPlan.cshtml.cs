using Meal_Planning.Core.Entities;
using Meal_Planning.Infrastructure.Persistence;
using Meal_Planning.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Meal_Planning.Application.Features.Areas.Identity.Pages.Meals.MealPlans
{
    [Authorize(Roles = "Member")]
    public class MealPlanModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<MealPlanModel> _logger;
        private readonly IDateTimeService _dateTimeService;


        public MealPlanModel(
       ApplicationDbContext context,
       IConfiguration configuration,
       IHttpClientFactory httpClientFactory,
       ILogger<MealPlanModel> logger,
       IDateTimeService dateTimeService)
        {
            _context = context;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _dateTimeService = dateTimeService;
        }

        [BindProperty]
        public string? MealPlan { get; set; }
        [BindProperty]
        public string? DietaryGoal { get; set; }
        [BindProperty]
        public string? DietaryRestrictions { get; set; }
        [BindProperty]
        public string? FavoriteFoods { get; set; }
        [BindProperty]
        public string? AvoidFoods { get; set; }
        [BindProperty]
        public string? Allergies { get; set; }
        [BindProperty]
        public int MealsPerDay { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            MealPlan = TempData["MealPlan"] as string;
            
            // Check if there's an error message from the result page
            if (TempData["ErrorMessage"] is string errorMsg)
            {
                ErrorMessage = errorMsg;
            }
            
            // Check user's trial status
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    await CheckAndSetTrialStatus(userId);
                }
            }
            
            return Page();
        }
        
        private async Task CheckAndSetTrialStatus(string userId)
        {
            // Check if user has an active subscription
            var hasActiveSubscription = await _context.Subscriptions
                .AnyAsync(s => s.UserId == userId && 
                           s.Status == "Active" && 
                           (s.ExpiresAt == null || s.ExpiresAt > DateTime.UtcNow));
            
            if (hasActiveSubscription)
            {
                // User has a paid subscription, no need to show trial status
                ViewData["TrialStatus"] = "Subscribed";
                return;
            }
            
            // Check if user is an admin (admins don't have trial limitations)
            if (User.IsInRole("Admin"))
            {
                ViewData["TrialStatus"] = "Admin";
                return;
            }
            
            // Get user preferences to check trial status
            var userPreference = await _context.Preferences.FirstOrDefaultAsync(p => p.UserID == userId);
            
            if (userPreference == null)
            {
                // Create user preferences with trial start date for new users
                userPreference = new UserPreference
                {
                    UserID = userId,
                    Likes = "",
                    Dislikes = "",
                    Allergies = "",
                    DietaryRestriction = "",
                    MealPlanGenerations = 0,
                    TrialStartDate = _dateTimeService.UtcNow // Set trial start date for new users using the service
                };
                _context.Preferences.Add(userPreference);
                await _context.SaveChangesAsync();
                
                // New trial just started
                ViewData["TrialStatus"] = "Active";
                ViewData["TrialDaysLeft"] = 7;
                return;
            }
            
            // If trial start date is not set, set it now
            if (!userPreference.TrialStartDate.HasValue)
            {
                userPreference.TrialStartDate = _dateTimeService.UtcNow;
                await _context.SaveChangesAsync();
                
                // Trial just started
                ViewData["TrialStatus"] = "Active";
                ViewData["TrialDaysLeft"] = 7;
                return;
            }
            
            // Calculate days left in trial
            DateTime trialEndDate = userPreference.TrialStartDate.Value.AddDays(7); // 7-day trial
            bool trialActive = _dateTimeService.UtcNow < trialEndDate;
            int daysLeft = Math.Max(0, (int)(trialEndDate - _dateTimeService.UtcNow).TotalDays);
            
            if (trialActive)
            {
                ViewData["TrialStatus"] = "Active";
                ViewData["TrialDaysLeft"] = daysLeft;
            }
            else
            {
                ViewData["TrialStatus"] = "Expired";
                ViewData["TrialDaysLeft"] = 0;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
                return RedirectToPage("/Areas/Identity/Pages/Account/Auths/Login", new { area = "Identity" });

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return RedirectToPage("/Areas/Identity/Pages/Account/Auths/Login", new { area = "Identity" });
                
            // Check subscription or trial status first
            await CheckAndSetTrialStatus(userId);
            var trialStatus = ViewData["TrialStatus"]?.ToString();
            
            if (trialStatus == "Expired")
            {
                return RedirectToPage("/Areas/Identity/Pages/Payments/MealPlanPayments");
            }

            // Handle multiple dietary restrictions (checkboxes)
            var restrictions = Request.Form["DietaryRestrictions"];
            DietaryRestrictions = string.Join(", ", restrictions.Where(r => !string.IsNullOrWhiteSpace(r)));

            // Append custom restriction if provided
            var customRestriction = Request.Form["DietaryRestrictions"].Count > 1
                ? Request.Form["DietaryRestrictions"][^1]
                : null;
            if (!string.IsNullOrWhiteSpace(customRestriction) && !DietaryRestrictions.Contains(customRestriction))
            {
                DietaryRestrictions = string.IsNullOrWhiteSpace(DietaryRestrictions)
                    ? customRestriction
                    : $"{DietaryRestrictions}, {customRestriction}";
            }

            // Save or update preferences
            var preferences = await _context.Preferences.FirstOrDefaultAsync(p => p.UserID == userId);
            if (preferences == null)
            {
                preferences = new UserPreference
                {
                    UserID = userId,
                    Likes = FavoriteFoods ?? "",
                    Dislikes = AvoidFoods ?? "",
                    Allergies = Allergies ?? "",
                    DietaryRestriction = DietaryRestrictions ?? "",
                    MealPlanGenerations = 0 // initialize
                };
                _context.Preferences.Add(preferences);
            }

            // Identify "you" (change this to your actual email or user ID)
            var myEmail = "freetesting@gmail.com"; // <-- Set your email here
            var myUserId = "8778acfe-569f-407f-bd76-991e7c60450f";  // <-- Or set your user ID here

            var userEmail = User.Identity?.Name; // Or fetch from claims if needed

            // Check if user is an admin, special user, or subscribed user
            var isAdmin = User.IsInRole("Admin");
            var isSpecialUser = userEmail == myEmail || userId == myUserId;
            var hasActiveSubscription = trialStatus == "Subscribed";
            
            // No limits for admins, subscribers, and special users
            if (!isAdmin && !isSpecialUser && !hasActiveSubscription && trialStatus == "Active") 
            {
                // For trial users: limited to 2 generations during trial period
                if (preferences.MealPlanGenerations >= 2)
                {
                    ErrorMessage = "You have reached your free meal plan generation limit (2). Please upgrade to a paid plan to generate more.";
                    return Page();
                }
            }

            preferences.Likes = FavoriteFoods ?? "";
            preferences.Dislikes = AvoidFoods ?? "";
            preferences.Allergies = Allergies ?? "";
            preferences.DietaryRestriction = DietaryRestrictions ?? "";

            await _context.SaveChangesAsync();

            // Generate meal plan
            MealPlan = await GenerateMealPlan(DietaryGoal ?? "", DietaryRestrictions ?? "", Allergies ?? "", FavoriteFoods ?? "", AvoidFoods ?? "", MealsPerDay);

            if (string.IsNullOrWhiteSpace(MealPlan))
            {
                ErrorMessage = ErrorMessage ?? "Failed to generate meal plan. Please try again.";
                return Page();
            }

            // Increment generation count
            preferences.MealPlanGenerations++;

            // Get recipe instructions from TempData
            var recipeInstructions = TempData["RecipeInstructions"] as string;
            
            // Save the generated meal plan to the database
            var mealPlanEntity = new MealPlan
            {
                UserID = userId,
                PlanData = MealPlan,
                RecipeInstructions = recipeInstructions,
                GeneratedAt = DateTime.Now,
                PlanJson = "", // Optionally serialize structured data here
                ParameterJson = "", // Optionally serialize parameters here
                Notes = "",
                IsFavorite = false
            };
            _context.MealPlanResults.Add(mealPlanEntity);

            await _context.SaveChangesAsync();

            TempData["MealPlan"] = MealPlan;
            TempData["RecipeInstructions"] = recipeInstructions;
            return RedirectToPage("MealPlanResult");
        }

        private async Task<string> GenerateMealPlan(
    string dietaryGoal,
    string dietaryRestrictions,
    string allergies,
    string favoriteFoods,
    string avoidFoods,
    int mealsPerDay)
{
    const string apiUrl = "https://openrouter.ai/api/v1/chat/completions";
    var httpClient = _httpClientFactory.CreateClient();

    // **Required headers for OpenRouter (Free Tier)**
    var apiKey = _configuration["OpenRouter:ApiKey"];
    var referer = _configuration["OpenRouter:Referer"] ?? "http://localhost:5001";
    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    httpClient.DefaultRequestHeaders.Add("HTTP-Referer", referer); // Required for free tier

    var prompt = $@"
Generate a **7-day meal plan** with concise meal descriptions in this **exact format**:

**Dietary Needs:**
- Goal: {dietaryGoal}
- Restrictions: {dietaryRestrictions}
- Allergies: {allergies}
- Favorite Foods: {favoriteFoods}
- Avoid: {avoidFoods}
- Meals/Day: {mealsPerDay}

**Output Format (STRICT):**
Day 1:
- Breakfast: [Meal]
- Lunch: [Meal]
- Dinner: [Meal]

Day 2:
- Breakfast: [Meal]
- Lunch: [Meal]
- Dinner: [Meal]

... (continue for 7 days)

**Rules:**
1. Only return the meal plan, no extra text.
2. Use simple descriptions.
3. No headers/footers.
";

    var requestBody = new
    {
        model = "mistralai/mistral-7b-instruct", // Free model
        messages = new[]
        {
            new { role = "user", content = prompt }
        },
        max_tokens = 1000
    };

    try
    {
        // First API call to generate the meal plan
        var response = await httpClient.PostAsJsonAsync(apiUrl, requestBody);
        var responseString = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("OpenRouter Error: {Response}", responseString);
            return string.Empty;
        }

        var jsonDoc = JsonDocument.Parse(responseString);
        if (jsonDoc.RootElement.TryGetProperty("choices", out var choices) &&
            choices.GetArrayLength() > 0 &&
            choices[0].TryGetProperty("message", out var message) &&
            message.TryGetProperty("content", out var mealPlanContent))
        {
            // We successfully got the meal plan
            var mealPlanText = mealPlanContent.GetString()?.Trim() ?? string.Empty;
            
            // Now make a second API call to get recipe instructions
            if (!string.IsNullOrEmpty(mealPlanText)) 
            {
                // Save the meal plan in the class property so we can use it in the instructions prompt
                MealPlan = mealPlanText;
                
                // Second prompt to generate recipe instructions
                var instructionsPrompt = $@"
For the following 7-day meal plan, provide brief, beginner-friendly cooking instructions for each meal. Format your response as a JSON string with the meal name as the key and instructions as the value.

Meal Plan:
{mealPlanText}

Instructions:
1. For each meal, provide 3-5 simple steps for preparation
2. Include approximate cooking time
3. List main cooking techniques (e.g. bake, sauté, boil)
4. Keep instructions concise but clear enough for novice cooks
5. Format as valid JSON with meal names as keys and instruction text as values

Example format:
{{
  ""Oatmeal with Berries"": ""1. Bring 1 cup water to boil. 2. Add 1/2 cup oats and cook for 5 min. 3. Top with fresh berries. (Cook time: 10 min)"",
  ""Grilled Chicken Salad"": ""1. Season chicken breast. 2. Grill for 6-7 min per side. 3. Slice and serve over mixed greens with dressing. (Cook time: 15 min)""
}}

Only respond with the JSON object, nothing else.";

                var instructionsRequestBody = new
                {
                    model = "mistralai/mistral-7b-instruct",
                    messages = new[]
                    {
                        new { role = "user", content = instructionsPrompt }
                    },
                    max_tokens = 2000
                };
                
                // Store the recipe instructions in TempData for later use
                try 
                {
                    var instructionsResponse = await httpClient.PostAsJsonAsync(apiUrl, instructionsRequestBody);
                    var instructionsResponseString = await instructionsResponse.Content.ReadAsStringAsync();
                    
                    if (instructionsResponse.IsSuccessStatusCode) 
                    {
                        var instructionsJsonDoc = JsonDocument.Parse(instructionsResponseString);
                        if (instructionsJsonDoc.RootElement.TryGetProperty("choices", out var instructionChoices) &&
                            instructionChoices.GetArrayLength() > 0 &&
                            instructionChoices[0].TryGetProperty("message", out var instructionMessage) &&
                            instructionMessage.TryGetProperty("content", out var instructionsContent))
                        {
                            var instructionsText = instructionsContent.GetString()?.Trim() ?? string.Empty;
                            if (!string.IsNullOrEmpty(instructionsText))
                            {
                                TempData["RecipeInstructions"] = instructionsText;
                            }
                        }
                    }
                    else
                    {
                        _logger.LogError("OpenRouter Error getting instructions: {Response}", instructionsResponseString);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting recipe instructions");
                }
            }
            
            return mealPlanText;
        }

        _logger.LogError("Failed to parse OpenRouter response.");
        return string.Empty;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "OpenRouter API Error");
        return string.Empty;
    }
}

    }
}