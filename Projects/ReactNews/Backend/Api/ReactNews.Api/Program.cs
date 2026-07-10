using Microsoft.AspNetCore.Authentication.Cookies;
using ReactNews.Application;
using ReactNews.Infrastructure;
using ReactNews.Infrastructure.Identity;
using ReactNews.Infrastructure.Options.AdminSeed;
using ReactNews.Infrastructure.Options.NewsApi;

var builder = WebApplication.CreateBuilder(args);

// What: configures predictable console logging for local and container runs.
// Why: console logging avoids host-specific log sinks such as Windows EventLog,
// which can require permissions that are unrelated to the application.
// How: clears the host defaults and adds console logging only.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// What: read Docker secrets from /run/secrets when running through compose.
// Why: Docker compose maps NEWSAPI_KEY into a file named NewsApi__ApiKey, which
// ASP.NET configuration understands as NewsApi:ApiKey.
// How: KeyPerFile adds each secret file as a configuration key.
builder.Configuration.AddKeyPerFile("/run/secrets", optional: true);

// What: choose a stable local SQLite path for ReactNews persistence.
// Why: relative SQLite paths can point at different folders depending on whether
// the app starts from Visual Studio, Start.ps1, dotnet run, or Docker. Building
// the path from ContentRootPath keeps the database beside the API project in
// local development and under /app/Data in the container.
// How: if no connection string is already configured, create Data/reactnews.db
// below the API content root and expose it through normal configuration.
var databaseFolder = Path.Combine(builder.Environment.ContentRootPath, "Data");
Directory.CreateDirectory(databaseFolder);

if (string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("ReactNews")))
{
    builder.Configuration["ConnectionStrings:ReactNews"] =
        $"Data Source={Path.Combine(databaseFolder, "reactnews.db")}";
}

builder.Services.AddControllers();
builder.Services.AddProblemDetails();

// What: configure browser cookie authentication for ReactNews accounts.
// How: the API issues an HttpOnly auth cookie after login/register and ASP.NET
// reads that cookie on later requests to build User claims.
// Why: cookie auth keeps browser credentials out of JavaScript-managed storage
// while still allowing the API to authorize requests with standard claims.
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "ReactNews.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.LoginPath = "/api/auth/login";
        options.AccessDeniedPath = "/api/auth/denied";
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();

// What: bind NewsApi configuration in the API project.
// Why: API owns runtime configuration, while Infrastructure owns how that
// configuration is used to call NewsAPI.
// How: Infrastructure receives IOptions<NewsApiOptions> through dependency
// injection when it creates the NewsAPI provider.
builder.Services.Configure<NewsApiOptions>(
    builder.Configuration.GetSection(NewsApiOptions.SectionName));
builder.Services.Configure<AdminSeedOptions>(
    builder.Configuration.GetSection(AdminSeedOptions.SectionName));

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        // What: allows only the expected local Vite frontend origins.
        // Why: explicit origins keep browser access scoped to known clients
        // while still allowing credentialed cookie requests.
        // How: Vite commonly uses localhost or 127.0.0.1 on port 5173.
        // Playwright uses 127.0.0.1:5174 so E2E tests can run without
        // colliding with a manually started development frontend.
        policy
            .WithOrigins("http://localhost:5173", "http://127.0.0.1:5173", "http://127.0.0.1:5174")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// What: register clean architecture layers.
// Why: Program.cs should compose the app, not contain business logic.
// How: Application registers use cases; Infrastructure registers concrete
// implementations for external systems and storage.
builder.Services.AddReactNewsApplication();
builder.Services.AddReactNewsInfrastructure(builder.Configuration);

var app = builder.Build();

// What: apply pending SQLite schema migrations when the app starts.
// Why: ReactNews now has real account, saved article, preference, and editorial
// data, so schema changes should be tracked by EF Core migration files instead
// of ad-hoc startup SQL.
// How: Infrastructure owns the EF Core DbContext and exposes this migration
// helper so the API still composes the app without directly depending on EF Core.
app.Services.MigrateReactNewsDatabase();
app.Services.EnsureSeedAdminCreated();

app.UseExceptionHandler();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

/// <summary>
/// What: exposes the top-level API Program type to integration tests.
/// How: C# top-level statements generate a Program class automatically; this
/// partial declaration gives WebApplicationFactory a public type to reference.
/// Why: integration tests need to boot the real API pipeline without changing
/// runtime behavior for Visual Studio, Start.ps1, Docker, or dotnet run.
/// </summary>
public partial class Program
{
}
