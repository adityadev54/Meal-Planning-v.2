using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Meal_Planning.Core.Entities;

[Authorize(Roles = "Developer")]
public class DeveloperIndexModel : PageModel
{
    private static Dictionary<string, bool> _featureFlags = new()
    {
        { "EnableBetaUI", false },
        { "EnableLogging", true },
        { "EnableExperimentalAPI", false }
    };

    public string AppVersion { get; set; } = string.Empty;
    public string EnvironmentName { get; set; } = string.Empty;
    public string ServerTime { get; set; } = string.Empty;
    public int UserCount { get; set; }
    public int ActiveFeatureFlags { get; set; }
    public List<string> RecentLogs { get; set; } = new List<string>();
    public Dictionary<string, bool> FeatureFlags => _featureFlags;

    // New: Developer feature options
    public List<DeveloperFeature> DeveloperFeatures { get; set; } = new()
    {
        new DeveloperFeature { Name = "API Key Management", Description = "Manage your API keys for programmatic access.", Page = "/Account/Devs/ApiKeys" },
        new DeveloperFeature { Name = "Data Import/Export", Description = "Import or export your data in CSV or JSON format.", Page = "/Account/Devs/ImportExport" },
        new DeveloperFeature { Name = "Webhook Console", Description = "Manage and test your webhook endpoints.", Page = "/Account/Devs/Webhooks" },
        new DeveloperFeature { Name = "API Documentation", Description = "View REST API docs, authentication, and code samples.", Page = "/Areas/Identity/Pages/Account/Devs/APIDocs" }
    };

    private readonly UserManager<ApplicationUser> _userManager;

    public DeveloperIndexModel(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public void OnGet()
    {
        LoadDiagnostics();
        LoadStats();
        LoadLogs();
    }

    public IActionResult OnPostRefreshLogs()
    {
        LoadDiagnostics();
        LoadStats();
        LoadLogs();
        return Page();
    }

    public IActionResult OnPostToggleFeature(List<string> flags)
    {
        foreach (var key in _featureFlags.Keys.ToList())
        {
            _featureFlags[key] = flags != null && flags.Contains(key);
        }
        LoadDiagnostics();
        LoadStats();
        LoadLogs();
        return Page();
    }

    private void LoadDiagnostics()
    {
        AppVersion = typeof(DeveloperIndexModel).Assembly.GetName().Version?.ToString() ?? "N/A";
        EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown";
        ServerTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

    private void LoadStats()
    {
        UserCount = _userManager.Users.Count();
        ActiveFeatureFlags = _featureFlags.Count(f => f.Value);
    }

    private void LoadLogs()
    {
        // Example: read last 50 lines from a log file (adjust path as needed)
        var logPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "app.log");
        if (System.IO.File.Exists(logPath))
        {
            RecentLogs = System.IO.File.ReadLines(logPath).Reverse().Take(50).Reverse().ToList();
        }
        else
        {
            RecentLogs = new List<string> { "No logs found." };
        }
    }

    // Helper class for developer features
    public class DeveloperFeature
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Page { get; set; }
    }
}