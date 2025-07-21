using Meal_Planning.Core.Entities;
using Meal_Planning.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

[Authorize(Roles = "Admin")]
public class ManageRolesModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ApplicationDbContext _context;
    private readonly ILogger _logger;

    public ManageRolesModel(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ApplicationDbContext context,
        ILogger<ManageRolesModel> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _context = context;
        _logger = logger;
    }

    public List<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    public Dictionary<string, List<string>> UserRoles { get; set; } = new();
    public List<string> Roles { get; set; } = new List<string>();
    public List<string> AvailableModels { get; set; } = new List<string>();

    [BindProperty]
    [Required(ErrorMessage = "Please select a user")]
    public string SelectedUserId { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Please select a role")]
    public string SelectedRole { get; set; } = string.Empty;

    public async Task OnGetAsync()
    {
        try
        {
            _logger.LogInformation("Loading ManageRoles page");
            Users = await _userManager.Users.OrderBy(u => u.Email).ToListAsync();
            Roles = await _roleManager.Roles
                .Where(r => r.Name != null)
                .Select(r => r.Name!)
                .OrderBy(name => name)
                .ToListAsync();

            UserRoles = new Dictionary<string, List<string>>();
            foreach (var user in Users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                UserRoles[user.Id] = roles.ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load ManageRoles data");
            TempData["ErrorMessage"] = $"Failed to load data: {ex.Message}";
        }
    }

    private async Task<List<string>> GetAvailableOllamaModelsAsync()
    {
        try
        {
            _logger.LogInformation("Fetching models from Ollama at http://localhost:11434/api/tags");
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            var response = await client.GetAsync("http://localhost:11434/api/tags");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Ollama API error: {StatusCode}, {Error}", response.StatusCode, errorContent);
                return new List<string>();
            }

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);
            var models = json.RootElement.GetProperty("models")
                .EnumerateArray()
                .Select(m => m.GetProperty("name").GetString())
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();

            if (!models.Any())
            {
                _logger.LogWarning("Ollama API returned no models");
                return new List<string>();
            }

            _logger.LogInformation("Fetched {Count} models: {Models}", models.Count, string.Join(", ", models));
            return models!;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to Ollama service");
            TempData["ErrorMessage"] = "Cannot connect to Ollama service. Ensure it is running.";
            return new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch Ollama models");
            TempData["ErrorMessage"] = "Failed to fetch AI models: " + ex.Message;
            return new List<string>();
        }
    }


    public async Task<List<string>> GetUserRolesAsync(ApplicationUser user)
    {
        return user != null ? (await _userManager.GetRolesAsync(user)).ToList() : new List<string>();
    }

    private static readonly HashSet<string> ProtectedUserIds = new()
    {
        "c4fd03fd-4d17-462f-97f4-86a3ce2219e4"
    };

    private bool IsProtectedUser(string userId)
    {
        return ProtectedUserIds.Contains(userId);
    }

    public async Task<IActionResult> OnPostAssignRoleAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        if (IsProtectedUser(SelectedUserId))
        {
            TempData["ErrorMessage"] = "Cannot modify roles for protected users";
            return RedirectToPage();
        }

        var user = await _userManager.FindByIdAsync(SelectedUserId);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found";
            return RedirectToPage();
        }

        var result = await _userManager.AddToRoleAsync(user, SelectedRole);
        if (result.Succeeded)
        {
            TempData["StatusMessage"] = $"Role '{SelectedRole}' assigned successfully";
        }
        else
        {
            TempData["ErrorMessage"] = string.Join(", ", result.Errors.Select(e => e.Description));
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRemoveRoleAsync()
    {
        if (string.IsNullOrEmpty(SelectedUserId) || IsProtectedUser(SelectedUserId))
            return Forbid();

        var user = await _userManager.FindByIdAsync(SelectedUserId);
        if (user != null && !string.IsNullOrEmpty(SelectedRole) && await _userManager.IsInRoleAsync(user, SelectedRole))
        {
            await _userManager.RemoveFromRoleAsync(user, SelectedRole);
            TempData["StatusMessage"] = $"Role '{SelectedRole}' removed successfully";
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostResetPasswordAsync()
    {
        if (Request.Headers["Content-Type"] == "application/json")
        {
            // AJAX request
            if (string.IsNullOrEmpty(SelectedUserId) || IsProtectedUser(SelectedUserId))
                return new JsonResult(new { success = false, message = "Operation not allowed for this user" });

            var user = await _userManager.FindByIdAsync(SelectedUserId);
            if (user == null)
                return new JsonResult(new { success = false, message = "User not found" });

            try
            {
                // Generate a new random password
                var newPassword = GenerateRandomPassword();
                var removeResult = await _userManager.RemovePasswordAsync(user);
                var addResult = await _userManager.AddPasswordAsync(user, newPassword);
                
                if (addResult.Succeeded)
                {
                    // Set flag for required password change on next login
                    user.PasswordChangeRequired = true;
                    await _userManager.UpdateAsync(user);
                    
                    _logger.LogInformation("Password reset for user {Email}, temporary password generated", user.Email);
                    
                    return new JsonResult(new { 
                        success = true, 
                        tempPassword = newPassword,
                        userEmail = user.Email 
                    });
                }
                else
                {
                    return new JsonResult(new { 
                        success = false, 
                        message = string.Join(", ", addResult.Errors.Select(e => e.Description)) 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for user {UserId}", SelectedUserId);
                return new JsonResult(new { success = false, message = "An error occurred during password reset" });
            }
        }
        else
        {
            // Traditional form submit
            if (string.IsNullOrEmpty(SelectedUserId) || IsProtectedUser(SelectedUserId))
                return Forbid();

            var user = await _userManager.FindByIdAsync(SelectedUserId);
            if (user != null)
            {
                // Generate a new random password
                var newPassword = GenerateRandomPassword();
                var removeResult = await _userManager.RemovePasswordAsync(user);
                var addResult = await _userManager.AddPasswordAsync(user, newPassword);
                if (addResult.Succeeded)
                {
                    // Set flag for required password change on next login
                    user.PasswordChangeRequired = true;
                    await _userManager.UpdateAsync(user);
                    
                    // Send the new password to the user's email
                    await SendPasswordResetEmailAsync(user.Email, newPassword);
                    TempData["StatusMessage"] = $"Password reset successfully. Temporary password: {newPassword}";
                }
                else
                {
                    TempData["ErrorMessage"] = string.Join(", ", addResult.Errors.Select(e => e.Description));
                }
            }
            return RedirectToPage();
        }
    }
    
    // New handler to send password reset email
    public async Task<IActionResult> OnPostSendPasswordEmailAsync([FromBody] PasswordEmailModel model)
    {
        if (string.IsNullOrEmpty(model.SelectedUserId) || IsProtectedUser(model.SelectedUserId))
            return new JsonResult(new { success = false, message = "Operation not allowed for this user" });
            
        var user = await _userManager.FindByIdAsync(model.SelectedUserId);
        if (user == null)
            return new JsonResult(new { success = false, message = "User not found" });
            
        if (string.IsNullOrEmpty(model.TempPassword))
            return new JsonResult(new { success = false, message = "No temporary password provided" });
            
        try
        {
            // Send the password to the user's email
            await SendPasswordResetEmailAsync(user.Email, model.TempPassword);
            _logger.LogInformation("Password reset email sent to {Email}", user.Email);
            return new JsonResult(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending password reset email to {Email}", user.Email);
            return new JsonResult(new { success = false, message = "Failed to send email" });
        }
    }
    
    public class PasswordEmailModel
    {
        public string SelectedUserId { get; set; } = string.Empty;
        public string TempPassword { get; set; } = string.Empty;
    }

    private string GenerateRandomPassword(int length = 12)
    {
        const string valid = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@$?_-";
        var res = new StringBuilder();
        var rnd = new Random();
        while (0 < length--)
            res.Append(valid[rnd.Next(valid.Length)]);
        return res.ToString();
    }

    private async Task SendPasswordResetEmailAsync(string? email, string newPassword)
    {
        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("Cannot send password reset email: email address is null or empty");
            await Task.CompletedTask;
            return;
        }
        
        // Use your email sender service here. Example:
        // await _emailSender.SendEmailAsync(email, "Your password has been reset", $"Your new password is: {newPassword}");
        // For now, just log it (replace with actual email logic)
        _logger.LogInformation("Password for {Email} reset to: {Password}", email, newPassword);
        
        // TODO: Implement actual email sending logic
        // This would typically include:
        // 1. HTML email template with company branding
        // 2. Instructions for login with the temporary password
        // 3. Clear explanation that they'll need to change the password immediately
        // 4. Security recommendations
        
        await Task.CompletedTask;
    }

    public async Task<IActionResult> OnPostLockUserAsync()
    {
        if (string.IsNullOrEmpty(SelectedUserId) || IsProtectedUser(SelectedUserId))
            return Forbid();

        var user = await _userManager.FindByIdAsync(SelectedUserId);
        if (user != null)
        {
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            TempData["StatusMessage"] = "User account locked";
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUnlockUserAsync()
    {
        if (string.IsNullOrEmpty(SelectedUserId) || IsProtectedUser(SelectedUserId))
            return Forbid();

        var user = await _userManager.FindByIdAsync(SelectedUserId);
        if (user != null)
        {
            await _userManager.SetLockoutEndDateAsync(user, null);
            TempData["StatusMessage"] = "User account unlocked";
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteUserAsync()
    {
        if (string.IsNullOrEmpty(SelectedUserId) || IsProtectedUser(SelectedUserId))
            return Forbid();

        var user = await _userManager.FindByIdAsync(SelectedUserId);
        if (user != null)
        {
            await _userManager.DeleteAsync(user);
            TempData["StatusMessage"] = "User deleted successfully";
        }
        return RedirectToPage();
    }

    public PartialViewResult OnGetGMUsersPartial()
    {
        var users = _userManager.Users.ToList();
        return Partial("_GMUsersPartial", users);
    }
}