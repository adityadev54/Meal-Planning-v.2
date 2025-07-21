using Meal_Planning.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace Meal_Planning.Application.Features.Areas.Identity.Pages.Feedback
{
    [Authorize]
    public class SubmitModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public SubmitModel(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [BindProperty]
        public Meal_Planning.Core.Entities.Feedback Feedback { get; set; } = new Meal_Planning.Core.Entities.Feedback();

        [BindProperty]
        public IFormFile? Attachment { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
{
    // Set UserId before validation!
    Feedback.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    Feedback.SubmittedAt = DateTime.UtcNow;

    if (!ModelState.IsValid)
    {
        foreach (var kvp in ModelState)
        {
            foreach (var error in kvp.Value.Errors)
            {
                System.Diagnostics.Debug.WriteLine($"Key: {kvp.Key}, Error: {error.ErrorMessage}");
            }
        }
        return Page();
    }

    if (Attachment != null && Attachment.Length > 0)
    {
        using (var memoryStream = new MemoryStream())
        {
            await Attachment.CopyToAsync(memoryStream);
            Feedback.AttachmentFileName = Attachment.FileName;
            Feedback.AttachmentData = memoryStream.ToArray();
        }
    }

    _context.Feedbacks.Add(Feedback);
    await _context.SaveChangesAsync();

    TempData["SuccessMessage"] = "Thank you for your feedback!";
    return RedirectToPage("/Index");
}
    }
}