using Meal_Planning.Application.Features.Areas.Identity.Pages.Meals.MealPlans;
using Meal_Planning.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Meal_Planning.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserPreference> Preferences { get; set; }
    public DbSet<MealPlan> MealPlanResults { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }
    public DbSet<AIGenerationLog> AIGenerationLogs { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<FavoriteStore> FavoriteStores { get; set; }
    public DbSet<UserSearchLog> UserSearchLogs { get; set; }
    public DbSet<SupportedLanguage> SupportedLanguages { get; set; }
    public DbSet<VisitorLog> VisitorLogs { get; set; }
    public DbSet<GroceryItem> GroceryItems { get; set; }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<UserPreference>()
            .HasOne(p => p.User)
            .WithMany(u => u.Preferences)
            .HasForeignKey(p => p.UserID);

        builder.Entity<MealPlan>()
            .ToTable("MealPlans")
            .HasOne(m => m.User)
            .WithMany(u => u.MealPlanResults)
            .HasForeignKey(m => m.UserID);

        builder.Entity<AIGenerationLog>()
            .HasOne(a => a.User)
            .WithMany(u => u.AIGenerationLogs)
            .HasForeignKey(a => a.UserId);

        builder.Entity<Subscription>().HasKey(s => s.Id);
        builder.Entity<Subscription>().Property(s => s.Amount).HasColumnType("decimal(18,2)");
    }
}


