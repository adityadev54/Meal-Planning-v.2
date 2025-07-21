using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Meal_Planning.Core.Entities;
using Meal_Planning.Infrastructure.Persistence;
using Meal_Planning.Infrastructure.Seed;
using Meal_Planning.Infrastructure.Services;
using Meal_Planning.Infrastructure.Middleware;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Database connection
var connectionString = builder.Configuration.GetConnectionString("GMDbContextConnection")
    ?? throw new InvalidOperationException("Connection string 'GMDbContextConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Identity configuration (no email confirmation required)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    // Production password, lockout, and user settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
    
// Add external authentication providers
// Add authorization policies
builder.Services.AddAuthorization(options => {
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
});

builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        // Read Google OAuth credentials from configuration
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "google-client-id";
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "google-client-secret";
        options.CallbackPath = "/signin-google";
    })
    .AddApple(options =>
    {
        // Read Apple OAuth credentials from configuration
        options.ClientId = builder.Configuration["Authentication:Apple:ClientId"] ?? "apple-client-id";
        options.KeyId = builder.Configuration["Authentication:Apple:KeyId"] ?? "apple-key-id";
        options.TeamId = builder.Configuration["Authentication:Apple:TeamId"] ?? "apple-team-id";
        
        // The private key configuration would typically be set up like this:
        // options.UsePrivateKey(builder.Environment.ContentRootFileProvider
        //     .GetFileInfo(builder.Configuration["Authentication:Apple:PrivateKeyPath"] ?? "private-key.p8"));
        
        // For now, we'll use a simpler approach for the demo
        options.GenerateClientSecret = true;
        options.CallbackPath = "/signin-apple";
    });

// Email sender (replace with real sender in production)
builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, FakeEmailSender>();
builder.Services.AddSingleton<IPaymentService, MockPaymentService>(); // Register mock payment service
builder.Services.AddSingleton<IDateTimeService, DateTimeService>(); // Register DateTime service for consistent date handling
builder.Services.AddMemoryCache(); // Required for caching
builder.Services.AddScoped<SubscriptionService>(); // Register subscription service
builder.Services.AddHostedService<SubscriptionBackgroundService>(); // Register background service for subscriptions

// Add Stripe configuration
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
builder.Services.Configure<LocationIQSettings>(builder.Configuration.GetSection("LocationIQ"));
builder.Services.AddHttpClient();


// Language configuration is now handled in profile and database only

// MVC and Razor Pages
builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages()
    .AddRazorPagesOptions(options =>
    {
        // Add a custom root directory for Razor Pages
        options.RootDirectory = "/Application/Features";
        options.Conventions.AddAreaPageRoute(
            areaName: "Identity",
            pageName: "/Account/Auths/Login",
            route: "Identity/Account/Auths/Login"
        );
        
        // Add Admin area routes
        options.Conventions.AuthorizeAreaFolder("Admin", "/", "AdminPolicy");
        options.Conventions.AddAreaPageRoute(
            areaName: "Admin", 
            pageName: "/Analytics/Index", 
            route: "Admin/Analytics");
        options.Conventions.AddAreaPageRoute(
            areaName: "Admin",
            pageName: "/Index",
            route: "Admin");
    });

// Session configuration
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Areas/Identity/Pages/Account/Denial/AccessDenied";
});


var app = builder.Build();

// Configure StaticWebAssets
StaticWebAssetsLoader.UseStaticWebAssets(app.Environment, app.Configuration);

// Seed roles and admin user
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    await ApplicationRoleSeeder.SeedRolesAsync(roleManager, userManager);

    // Assign the specific user to the Admin role
    var adminUserId = "172f4082-fb16-48f0-b016-1014604cb2b0";
    var adminUser = await userManager.FindByIdAsync(adminUserId);
    if (adminUser != null && !await userManager.IsInRoleAsync(adminUser, "Admin"))
    {
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}

// Error handling and security
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // Enforce HTTP Strict Transport Security
}

app.UseHttpsRedirection();

// Configure static files with proper MIME types
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".webmanifest"] = "application/manifest+json";
provider.Mappings[".woff"] = "application/font-woff";
provider.Mappings[".woff2"] = "application/font-woff2";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});

app.UseSession();

app.UseRouting();

// Localization removed - now handled by user profile and database

// Track visitors through ngrok
app.UseVisitorTracking();

// Apply security middleware (commented out for now)
// app.UseMiddleware<SecurityHeadersMiddleware>();
// app.UseMiddleware<AntiXssMiddleware>();
// app.UseMiddleware<RateLimitingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();