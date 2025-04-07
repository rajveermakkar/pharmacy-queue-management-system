using Microsoft.EntityFrameworkCore;
using PharmacyQueue.Data;
using PharmacyQueue.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

// Create the web application builder - this is the starting point for configuring the application
var builder = WebApplication.CreateBuilder(args);

// Add MVC services to enable controllers and views for the web application
// This allows us to create controller classes that handle HTTP requests and return views
builder.Services.AddControllersWithViews();

// Configure session state to maintain data between requests from the same client
// This allows us to store temporary data like login status and form submissions
builder.Services.AddSession(options =>
{
    // Set session timeout to 30 minutes of inactivity
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    // Only allow the server to access cookie data, not client-side JavaScript
    options.Cookie.HttpOnly = true;
    // Mark this cookie as essential so it works even if the user doesn't consent to non-essential cookies
    options.Cookie.IsEssential = true;
});

// Register application services for dependency injection
// Register scoped services - a new instance is created for each HTTP request
builder.Services.AddScoped<QueueService>();     // Service for managing pharmacy queue operations
builder.Services.AddScoped<EmailService>();     // Service for sending email notifications

// Register background services that run continuously in the background
// These services perform operations on a timer or schedule
builder.Services.AddHostedService<ReminderService>();          // Service that sends appointment reminders
builder.Services.AddHostedService<AppointmentCleanupService>(); // Service that cleans up old appointment data

// Configure error handling and shutdown behavior for background services
// This prevents background service exceptions from crashing the entire application
builder.Services.Configure<HostOptions>(options =>
{
    // Continue running even if a background service throws an exception
    options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
    // Set maximum time to wait for graceful shutdown before forcing termination
    options.ShutdownTimeout = TimeSpan.FromSeconds(30);
});

// Configure cookie-based authentication
// This enables users to log in and maintain their authentication state across requests
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Set the URL where unauthenticated users will be redirected to log in
        options.LoginPath = "/Admin/Login";
        // Set the URL where users will be redirected on logout
        options.LogoutPath = "/Admin/Logout";
        // Set the URL where users will be redirected if they access a forbidden resource
        options.AccessDeniedPath = "/Home/AccessDenied";
        // Set how long the authentication cookie remains valid (8 hours)
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        // Reset expiration time with each request to extend session while active
        options.SlidingExpiration = true;
    });

// Configure the database connection using MySQL
// Get the connection string from appsettings.json or environment variables
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// Register the database context service with Entity Framework Core
builder.Services.AddDbContext<PharmacyDbContext>(options =>
    // Configure MySQL as the database provider with automatic version detection
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Build the application - this finalizes the configuration and prepares the app to run
var app = builder.Build();

// Configure the HTTP request pipeline - the sequence of middleware components that process requests
if (!app.Environment.IsDevelopment())
{
    // In production, redirect errors to the /Home/Error action
    app.UseExceptionHandler("/Home/Error");

    // Skip HTTPS redirection if running in a Docker container (often handled by reverse proxy)
    if (!string.Equals(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), "true"))
    {
        // Enable HTTP Strict Transport Security to enforce HTTPS connections
        app.UseHsts();
        // Redirect HTTP requests to HTTPS
        app.UseHttpsRedirection();
    }
}
else
{
    // In development, use HTTPS redirection but show detailed error pages
    app.UseHttpsRedirection();
}

// Enable serving static files like CSS, JavaScript, and images from wwwroot folder
app.UseStaticFiles();

// Enable endpoint routing - determines which endpoint handles each request
app.UseRouting();

// Enable session state middleware - must be after UseRouting and before UseEndpoints
app.UseSession();

// Enable authentication middleware - checks if users are authenticated
app.UseAuthentication();
// Enable authorization middleware - checks if authenticated users have permission
app.UseAuthorization();

// Map static assets for middleware like client-side blazor
app.MapStaticAssets();

// Configure the default route pattern for controller actions
// Format: /[Controller]/[Action]/[optional-id]
// Default: /Home/Index if no specific route is provided
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Start the application and begin listening for requests
app.Run();
