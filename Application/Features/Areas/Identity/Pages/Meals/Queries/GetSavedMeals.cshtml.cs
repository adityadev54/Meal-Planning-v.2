using Meal_Planning.Core.Entities;
using Meal_Planning.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Meal_Planning.Areas.Identity.Pages.Meals
{
    [Authorize]
    public class SavedMealsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public SavedMealsModel(ApplicationDbContext context)
        {
            _context = context;
            SavedMealPlans = new List<SavedMealPlanViewModel>();
        }

        public List<SavedMealPlanViewModel> SavedMealPlans { get; set; } = new();
        
        public async Task<IActionResult> OnPostUpdateGroceryItemAsync(int planId, string item, bool completed)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return BadRequest();
            
            var groceryItem = await _context.GroceryItems
                .FirstOrDefaultAsync(g => g.UserId == userId && g.PlanId == planId && g.ItemName == item);
                
            if (groceryItem == null)
            {
                groceryItem = new GroceryItem
                {
                    UserId = userId,
                    PlanId = planId,
                    ItemName = item,
                    IsCompleted = completed,
                    CreatedAt = DateTime.UtcNow
                };
                _context.GroceryItems.Add(groceryItem);
            }
            else
            {
                groceryItem.IsCompleted = completed;
            }
            
            await _context.SaveChangesAsync();
            return new JsonResult(new { success = true });
        }
        
        public async Task<IActionResult> OnPostAddGroceryItemAsync(int planId, string item)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || string.IsNullOrWhiteSpace(item)) return BadRequest();
            
            var existingItem = await _context.GroceryItems
                .FirstOrDefaultAsync(g => g.UserId == userId && g.PlanId == planId && g.ItemName.ToLower() == item.ToLower());
                
            if (existingItem == null)
            {
                var groceryItem = new GroceryItem
                {
                    UserId = userId,
                    PlanId = planId,
                    ItemName = item.Trim(),
                    IsCompleted = false,
                    CreatedAt = DateTime.UtcNow
                };
                _context.GroceryItems.Add(groceryItem);
                await _context.SaveChangesAsync();
            }
            
            return new JsonResult(new { success = true });
        }
        
        public async Task<IActionResult> OnGetGroceryItemsAsync(int planId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return BadRequest();
            
            var groceryItems = await _context.GroceryItems
                .Where(g => g.UserId == userId && g.PlanId == planId)
                .OrderBy(g => g.IsCompleted)
                .ThenBy(g => g.ItemName)
                .ToListAsync();
                
            return new JsonResult(groceryItems);
        }

        public void OnGet()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var query = Request.Query;
            string sort = query["sort"].ToString()?.ToLower() ?? "newest";
            string? dateStr = query["date"];
            DateTime? filterDate = null;
            if (!string.IsNullOrEmpty(dateStr) && DateTime.TryParse(dateStr, out var dt)) filterDate = dt.Date;

            if (!string.IsNullOrEmpty(userId))
            {
                var plansQuery = _context.MealPlanResults
                    .Where(m => m.UserID == userId);

                if (filterDate.HasValue)
                {
                    // Compare only the date part
                    plansQuery = plansQuery.Where(m => m.GeneratedAt.Date == filterDate.Value);
                }

                var plans = plansQuery.ToList();

                // Sorting
                if (sort == "oldest")
                {
                    plans = plans.OrderBy(m => m.GeneratedAt).ToList();
                }
                else if (sort == "meals")
                {
                    plans = plans.OrderByDescending(m => CountMeals(m.PlanData ?? "")).ToList();
                }
                else // newest (default)
                {
                    plans = plans.OrderByDescending(m => m.GeneratedAt).ToList();
                }

                SavedMealPlans = plans.Select(m => new SavedMealPlanViewModel
                {
                    PlanID = m.PlanID,
                    GeneratedAt = m.GeneratedAt,
                    PlanData = m.PlanData ?? "",
                    RecipeInstructions = m.RecipeInstructions ?? "",
                    ParsedDays = ParseMealPlan(m.PlanData ?? string.Empty)
                }).ToList();
            }
        }
        // Helper for sorting by meal count
        private int CountMeals(string planData)
        {
            if (string.IsNullOrEmpty(planData)) return 0;
            return System.Text.RegularExpressions.Regex.Matches(planData, @"(Breakfast|Lunch|Dinner)\s*[:-]", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Count;
        }
        
        public List<string> GetGroceryItems(string planData, string category)
        {
            if (string.IsNullOrEmpty(planData)) return new List<string>();
            
            var items = new HashSet<string>();
            var mealPlan = ParseMealPlan(planData);
            
            // Extract ingredients from the meal descriptions
            foreach (var day in mealPlan)
            {
                foreach (var meal in day.Meals)
                {
                    var ingredients = ExtractIngredientsFromMeal(meal.Food);
                    foreach (var ingredient in ingredients)
                    {
                        // Simple categorization of ingredients
                        if (ShouldIncludeInCategory(ingredient, category))
                        {
                            items.Add(ingredient);
                        }
                    }
                }
            }
            
            return items.OrderBy(i => i).ToList();
        }
        
        private bool ShouldIncludeInCategory(string ingredient, string category)
        {
            ingredient = ingredient.ToLower();
            
            switch (category.ToLower())
            {
                case "produce":
                    return IsProduceItem(ingredient);
                case "dairy":
                    return IsDairyItem(ingredient);
                case "proteins":
                    return IsProteinItem(ingredient);
                case "grains":
                    return IsGrainItem(ingredient);
                case "spices":
                    return IsSpiceItem(ingredient);
                case "other":
                    return !IsProduceItem(ingredient) && !IsDairyItem(ingredient) && 
                           !IsProteinItem(ingredient) && !IsGrainItem(ingredient) && 
                           !IsSpiceItem(ingredient);
                default:
                    return true;
            }
        }
        
        private bool IsProduceItem(string ingredient)
        {
            var produceItems = new[] { 
                "apple", "banana", "carrot", "spinach", "lettuce", "tomato", "potato", "onion", 
                "garlic", "cucumber", "bell pepper", "pepper", "broccoli", "cauliflower", 
                "zucchini", "squash", "eggplant", "avocado", "berries", "orange", "lemon",
                "lime", "pineapple", "mango", "kale", "celery", "cilantro", "parsley",
                "basil", "fruit", "vegetable"
            };
            
            return produceItems.Any(item => ingredient.Contains(item));
        }
        
        private bool IsDairyItem(string ingredient)
        {
            var dairyItems = new[] { 
                "milk", "cheese", "yogurt", "butter", "cream", "sour cream", "cream cheese",
                "cottage cheese", "ricotta", "mozzarella", "cheddar", "parmesan", "dairy"
            };
            
            return dairyItems.Any(item => ingredient.Contains(item));
        }
        
        private bool IsProteinItem(string ingredient)
        {
            var proteinItems = new[] { 
                "chicken", "beef", "pork", "turkey", "fish", "salmon", "tuna", "shrimp", "tofu",
                "eggs", "egg", "lentils", "beans", "bean", "chickpeas", "tempeh", "meat",
                "steak", "sausage", "bacon", "ground", "protein", "ham"
            };
            
            return proteinItems.Any(item => ingredient.Contains(item));
        }
        
        private bool IsGrainItem(string ingredient)
        {
            var grainItems = new[] { 
                "rice", "pasta", "bread", "oats", "quinoa", "barley", "flour", "cereal",
                "tortilla", "wrap", "noodle", "bagel", "bun", "roll", "wheat", "grain",
                "cracker", "crust", "pita", "couscous"
            };
            
            return grainItems.Any(item => ingredient.Contains(item));
        }
        
        private bool IsSpiceItem(string ingredient)
        {
            var spiceItems = new[] { 
                "salt", "pepper", "cumin", "paprika", "cinnamon", "nutmeg", "oregano", "thyme",
                "rosemary", "basil", "vanilla", "chili", "powder", "seasoning", "spice",
                "extract", "oil", "vinegar", "sauce", "condiment", "herb"
            };
            
            return spiceItems.Any(item => ingredient.Contains(item));
        }
        
        public NutritionalSummary GetNutritionalSummary(string planData)
        {
            var summary = new NutritionalSummary();
            if (string.IsNullOrEmpty(planData)) return summary;
            
            var mealPlan = ParseMealPlan(planData);
            int totalMeals = mealPlan.SelectMany(d => d.Meals).Count();
            
            // Calculate estimated nutrition based on meal descriptions
            foreach (var day in mealPlan)
            {
                foreach (var meal in day.Meals)
                {
                    var mealType = meal.Meal.ToLower();
                    var food = meal.Food.ToLower();
                    
                    // Simple nutrition estimation based on meal content
                    summary.EstimatedCalories += EstimateCalories(mealType, food);
                    summary.EstimatedProtein += EstimateProtein(mealType, food);
                    summary.EstimatedCarbs += EstimateCarbs(mealType, food);
                    summary.EstimatedFat += EstimateFat(mealType, food);
                    
                    // Track food groups
                    if (ContainsProteins(food)) summary.ProteinMeals++;
                    if (ContainsWholeGrains(food)) summary.WholeGrainMeals++;
                    if (ContainsVegetables(food)) summary.VegetableMeals++;
                    if (ContainsFruits(food)) summary.FruitMeals++;
                    if (ContainsDairy(food)) summary.DairyMeals++;
                }
            }
            
            // Convert totals to per-day averages
            int days = Math.Max(1, mealPlan.Count);
            summary.AverageCaloriesPerDay = summary.EstimatedCalories / days;
            summary.AverageProteinPerDay = summary.EstimatedProtein / days;
            summary.AverageCarbsPerDay = summary.EstimatedCarbs / days;
            summary.AverageFatPerDay = summary.EstimatedFat / days;
            
            // Calculate percentages for food groups
            if (totalMeals > 0)
            {
                summary.ProteinPercentage = (double)summary.ProteinMeals / totalMeals * 100;
                summary.WholeGrainPercentage = (double)summary.WholeGrainMeals / totalMeals * 100;
                summary.VegetablePercentage = (double)summary.VegetableMeals / totalMeals * 100;
                summary.FruitPercentage = (double)summary.FruitMeals / totalMeals * 100;
                summary.DairyPercentage = (double)summary.DairyMeals / totalMeals * 100;
            }
            
            return summary;
        }
        
        private int EstimateCalories(string mealType, string food)
        {
            int baseCalories = mealType switch
            {
                "breakfast" => 400,
                "lunch" => 600,
                "dinner" => 700,
                _ => 300
            };
            
            // Adjust based on meal content
            if (food.Contains("salad")) baseCalories -= 200;
            if (food.Contains("burger") || food.Contains("pizza")) baseCalories += 200;
            if (food.Contains("dessert") || food.Contains("cake") || food.Contains("cookie")) baseCalories += 150;
            if (food.Contains("grilled")) baseCalories -= 100;
            if (food.Contains("fried")) baseCalories += 150;
            
            return Math.Max(200, baseCalories); // Minimum of 200 calories
        }
        
        private int EstimateProtein(string mealType, string food)
        {
            int baseProtein = mealType switch
            {
                "breakfast" => 15,
                "lunch" => 25,
                "dinner" => 30,
                _ => 10
            };
            
            if (food.Contains("chicken") || food.Contains("turkey")) baseProtein += 10;
            if (food.Contains("beef") || food.Contains("steak")) baseProtein += 15;
            if (food.Contains("fish") || food.Contains("salmon")) baseProtein += 12;
            if (food.Contains("tofu") || food.Contains("beans")) baseProtein += 8;
            if (food.Contains("eggs")) baseProtein += 7;
            if (food.Contains("salad") && !food.Contains("chicken") && !food.Contains("fish")) baseProtein -= 5;
            
            return Math.Max(5, baseProtein);
        }
        
        private int EstimateCarbs(string mealType, string food)
        {
            int baseCarbs = mealType switch
            {
                "breakfast" => 50,
                "lunch" => 60,
                "dinner" => 55,
                _ => 30
            };
            
            if (food.Contains("pasta") || food.Contains("rice") || food.Contains("bread")) baseCarbs += 20;
            if (food.Contains("potato")) baseCarbs += 15;
            if (food.Contains("salad") && !food.Contains("pasta")) baseCarbs -= 15;
            if (food.Contains("low carb") || food.Contains("keto")) baseCarbs -= 30;
            
            return Math.Max(10, baseCarbs);
        }
        
        private int EstimateFat(string mealType, string food)
        {
            int baseFat = mealType switch
            {
                "breakfast" => 15,
                "lunch" => 20,
                "dinner" => 25,
                _ => 10
            };
            
            if (food.Contains("avocado")) baseFat += 10;
            if (food.Contains("cheese")) baseFat += 7;
            if (food.Contains("butter")) baseFat += 8;
            if (food.Contains("oil")) baseFat += 5;
            if (food.Contains("fried")) baseFat += 10;
            if (food.Contains("low fat")) baseFat -= 10;
            
            return Math.Max(5, baseFat);
        }
        
        private bool ContainsProteins(string food)
        {
            string[] proteins = { "chicken", "beef", "fish", "salmon", "tuna", "shrimp", "tofu", 
                                 "turkey", "pork", "eggs", "lentils", "beans", "steak", "meat" };
            return proteins.Any(p => food.Contains(p));
        }
        
        private bool ContainsWholeGrains(string food)
        {
            string[] wholeGrains = { "whole grain", "whole wheat", "brown rice", "oats", "quinoa", 
                                    "barley", "whole", "grain" };
            return wholeGrains.Any(wg => food.Contains(wg));
        }
        
        private bool ContainsVegetables(string food)
        {
            string[] vegetables = { "spinach", "kale", "broccoli", "cauliflower", "carrot", "pepper", 
                                  "tomato", "lettuce", "salad", "vegetable", "veggies" };
            return vegetables.Any(v => food.Contains(v));
        }
        
        private bool ContainsFruits(string food)
        {
            string[] fruits = { "apple", "banana", "orange", "berries", "strawberry", "blueberry", 
                               "fruit", "pineapple", "mango", "peach" };
            return fruits.Any(f => food.Contains(f));
        }
        
        private bool ContainsDairy(string food)
        {
            string[] dairy = { "milk", "cheese", "yogurt", "dairy", "cream", "butter" };
            return dairy.Any(d => food.Contains(d));
        }
        
        private List<string> ExtractIngredientsFromMeal(string mealDescription)
        {
            var ingredients = new HashSet<string>();
            var words = mealDescription.Split(new[] { ' ', ',', '.', ';', ':', '-', '(', ')', '\n' }, 
                StringSplitOptions.RemoveEmptyEntries);
            
            // Common food items to extract
            var commonFoodItems = new[] {
                "chicken", "beef", "pork", "turkey", "fish", "salmon", "tuna", "shrimp", "tofu", 
                "eggs", "milk", "cheese", "yogurt", "bread", "rice", "pasta", "potato", "tomato", 
                "onion", "garlic", "spinach", "kale", "broccoli", "carrot", "lettuce", "cucumber", 
                "avocado", "lemon", "lime", "apple", "banana", "orange", "berries", "oats",
                "quinoa", "beans", "lentils", "butter", "oil", "flour", "sugar", "salt", "pepper",
                "spices", "herbs"
            };
            
            // Extract common ingredients
            foreach (var word in words)
            {
                var cleanWord = word.Trim().ToLower();
                if (cleanWord.Length > 2) // Skip very short words
                {
                    foreach (var foodItem in commonFoodItems)
                    {
                        if (cleanWord.Contains(foodItem))
                        {
                            ingredients.Add(TitleCase(foodItem));
                            break;
                        }
                    }
                }
            }
            
            // Look for compound ingredients like "bell pepper" or "olive oil"
            var twoWordPhrases = new List<string>();
            for (int i = 0; i < words.Length - 1; i++)
            {
                twoWordPhrases.Add($"{words[i]} {words[i + 1]}".ToLower());
            }
            
            // Common two-word ingredients
            var twoWordIngredients = new[] {
                "bell pepper", "olive oil", "whole grain", "brown rice", "greek yogurt", "red onion",
                "green beans", "sweet potato", "peanut butter", "almond milk", "coconut milk"
            };
            
            foreach (var phrase in twoWordPhrases)
            {
                foreach (var twoWordIngredient in twoWordIngredients)
                {
                    if (phrase.Contains(twoWordIngredient))
                    {
                        ingredients.Add(TitleCase(twoWordIngredient));
                        break;
                    }
                }
            }
            
            return ingredients.ToList();
        }
        
        private string TitleCase(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            
            var words = text.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                if (!string.IsNullOrEmpty(words[i]))
                {
                    char[] letters = words[i].ToCharArray();
                    letters[0] = char.ToUpper(letters[0]);
                    words[i] = new string(letters);
                }
            }
            
            return string.Join(" ", words);
        }

        private List<ParsedMealDay> ParseMealPlan(string planData)
        {
            var result = new List<ParsedMealDay>();
            if (string.IsNullOrWhiteSpace(planData)) return result;

            var lines = planData.Split('\n')
                               .Select(l => l.Trim())
                               .Where(l => !string.IsNullOrEmpty(l))
                               .ToList();

            ParsedMealDay? currentDay = null;
            var dayPattern = new System.Text.RegularExpressions.Regex(
                @"^(Day\s*\d+|Monday|Tuesday|Wednesday|Thursday|Friday|Saturday|Sunday)[:\-]?",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );

            foreach (var line in lines)
            {
                // Check for day header
                var dayMatch = dayPattern.Match(line);
                if (dayMatch.Success)
                {
                    currentDay = new ParsedMealDay
                    {
                        Day = dayMatch.Groups[1].Value,
                        Meals = new List<(string, string)>()
                    };
                    result.Add(currentDay);
                    continue;
                }

                // Check for meal line (more flexible pattern)
                if (currentDay != null)
                {
                    var mealParts = line.Split(new[] { ':', '-' }, 2);
                    if (mealParts.Length == 2)
                    {
                        currentDay.Meals.Add((
                            mealParts[0].Trim(),
                            mealParts[1].Trim()
                        ));
                    }
                    else if (line.StartsWith("-"))
                    {
                        currentDay.Meals.Add((
                            "Note",
                            line.Substring(1).Trim()
                        ));
                    }
                }
            }

            return result;
        }
    }

    public class ParsedMealDay
    {
        public string Day { get; set; } = "";
        public List<(string Meal, string Food)> Meals { get; set; } = new();
    }

    public class SavedMealPlanViewModel
    {
        public int PlanID { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string PlanData { get; set; } = "";
        public string RecipeInstructions { get; set; } = "";
        public List<ParsedMealDay> ParsedDays { get; set; } = new();
        public NutritionalSummary? NutritionInfo { 
            get {
                // Create a temporary instance just to use the static methods
                // This is a bit of a hack but avoids changing our parsing methods to static
                var model = new SavedMealsModel(null!);
                return model.GetNutritionalSummary(PlanData);
            }
        }
    }
    
    public class NutritionalSummary
    {
        // Total estimates
        public int TotalCalories => EstimatedCalories;
        public int TotalProtein => EstimatedProtein;
        public int TotalCarbs => EstimatedCarbs;
        public int TotalFat => EstimatedFat;
        
        public int EstimatedCalories { get; set; } = 0;
        public int EstimatedProtein { get; set; } = 0;
        public int EstimatedCarbs { get; set; } = 0;
        public int EstimatedFat { get; set; } = 0;
        
        // Daily averages
        public int AverageCaloriesPerDay { get; set; } = 0;
        public int AverageProteinPerDay { get; set; } = 0;
        public int AverageCarbsPerDay { get; set; } = 0;
        public int AverageFatPerDay { get; set; } = 0;
        
        // Food group tracking
        public int ProteinMeals { get; set; } = 0;
        public int WholeGrainMeals { get; set; } = 0;
        public int VegetableMeals { get; set; } = 0;
        public int FruitMeals { get; set; } = 0;
        public int DairyMeals { get; set; } = 0;
        
        // Food groups for display
        public List<(string, int)> FoodGroups => new List<(string, int)>
        {
            ("Vegetables", VegetableMeals),
            ("Fruits", FruitMeals),
            ("Grains", WholeGrainMeals),
            ("Protein", ProteinMeals),
            ("Dairy", DairyMeals),
            ("Other", 0)
        };
        
        // Percentages of meals containing each food group
        public double ProteinPercentage { get; set; } = 0;
        public double WholeGrainPercentage { get; set; } = 0;
        public double VegetablePercentage { get; set; } = 0;
        public double FruitPercentage { get; set; } = 0;
        public double DairyPercentage { get; set; } = 0;
        
        // Macronutrient percentages
        public double ProteinCaloriePercentage => EstimatedCalories > 0 ? 
            Math.Round((EstimatedProtein * 4.0) / EstimatedCalories * 100) : 0;
            
        public double CarbCaloriePercentage => EstimatedCalories > 0 ? 
            Math.Round((EstimatedCarbs * 4.0) / EstimatedCalories * 100) : 0;
            
        public double FatCaloriePercentage => EstimatedCalories > 0 ? 
            Math.Round((EstimatedFat * 9.0) / EstimatedCalories * 100) : 0;
    }
}