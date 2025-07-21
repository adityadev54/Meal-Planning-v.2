using Meal_Planning.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Meal_Planning.Application.Features.Areas.Identity.Pages.Feedback
{
        [Authorize(Roles = "Admin")]
        public class ReviewModel : PageModel
        {
            private readonly ApplicationDbContext _context;

            public ReviewModel(ApplicationDbContext context)
            {
                _context = context;
            }

            public IList<Meal_Planning.Core.Entities.Feedback> Feedbacks { get; set; } = new List<Meal_Planning.Core.Entities.Feedback>();

            public void OnGet()
            {
                Feedbacks = _context.Feedbacks
                    .OrderByDescending(f => f.SubmittedAt)
                    .ToList();
            }
        }
    }
