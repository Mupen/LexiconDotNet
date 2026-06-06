using DataDrivenCaching.Application;
using DataDrivenCaching.Infrastructure;
using DataDrivenCaching.Infrastructure.DataStores;
using DataDrivenCaching.Infrastructure.Persistence;
using DataDrivenCaching.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// WHAT:
// The connection string points EF Core at a SQLite database file.
//
// WHY:
// SQLite gives the project a real backend source of truth without requiring
// SQL Server, Docker, or a separate database process. This matters because the
// project is about comparing authoritative data with temporary frontend data
// and cached copies.
var connectionString = builder.Configuration.GetConnectionString("DataDrivenCaching")
    ?? throw new InvalidOperationException("Connection string 'DataDrivenCaching' is missing.");

// WHAT:
// The Data folder is where the SQLite file will live.
//
// WHY:
// Keeping the database file inside the API project makes it easy for learners
// to find the physical file and understand that backend database data lives on
// disk, not in browser storage and not in JavaScript memory.
Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, "Data"));

builder.Services.AddProblemDetails();

// WHAT:
// AddDistributedMemoryCache gives ASP.NET Session a server-side place to store
// session values for this demo.
//
// WHY:
// The browser should not receive trusted account data. The browser receives a
// session cookie, and the server uses that cookie to find the backend session
// values. This teaches that cookies transport identity; they are not the whole
// identity record.
builder.Services.AddDistributedMemoryCache();

// WHAT:
// AddSession enables backend session state.
//
// WHY:
// Login state is trusted backend state. The session cookie is only a lookup key
// that connects a browser to server-side session data.
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".DataDrivenCaching.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.IdleTimeout = TimeSpan.FromMinutes(20);
});

builder.Services.AddDataDrivenCachingApplication();
builder.Services.AddDataDrivenCachingInfrastructure(connectionString);

var app = builder.Build();

app.UseExceptionHandler();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseSession();

// WHAT:
// EnsureCreated creates the SQLite schema when the project starts.
//
// WHY:
// Migrations are better for many production systems, but EnsureCreated keeps
// this early learning project easy to run. Later, when the data model grows,
// we can switch to migrations and use that as another teaching moment.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DataDrivenCachingDbContext>();
    await dbContext.Database.EnsureCreatedAsync();

    var userStore = scope.ServiceProvider.GetRequiredService<LabUserStore>();
    await userStore.SeedDemoUsersAsync();
}

// WHAT:
// This endpoint checks a username/password pair against backend-owned account
// data and writes trusted login state into ASP.NET Session.
//
// WHY:
// The browser sends credentials, but the backend decides whether they are
// correct. The password hash stays in SQLite and is never returned to the
// frontend. The session cookie only transports the browser's session id.
app.MapPost("/api/login", async (
    LoginRequest request,
    LabUserStore userStore,
    HttpContext httpContext,
    CancellationToken cancellationToken) =>
{
    var user = await userStore.FindByUserNameAsync(request.UserName, cancellationToken);

    if (user is null || !PasswordHashing.VerifyPassword(request.Password, user.PasswordHash))
    {
        return Results.Unauthorized();
    }

    httpContext.Session.SetInt32("UserId", user.Id);
    httpContext.Session.SetString("UserName", user.UserName);
    httpContext.Session.SetString("DisplayName", user.DisplayName);

    return Results.Ok(new SessionStatusResponse(
        IsLoggedIn: true,
        UserName: user.UserName,
        DisplayName: user.DisplayName,
        Explanation: "SQLite verified the account. ASP.NET Session now stores trusted login state on the backend."));
});

// WHAT:
// This endpoint reports the backend session state connected to the current
// browser request.
//
// WHY:
// JavaScript cannot read the HttpOnly session cookie, and it should not need
// to. The frontend asks the backend who is logged in, and the backend answers
// from session state.
app.MapGet("/api/session", (HttpContext httpContext) =>
{
    var userName = httpContext.Session.GetString("UserName");
    var displayName = httpContext.Session.GetString("DisplayName");

    return Results.Ok(new SessionStatusResponse(
        IsLoggedIn: userName is not null,
        UserName: userName,
        DisplayName: displayName,
        Explanation: userName is null
            ? "No backend session user exists for this browser request."
            : "The browser sent a session cookie. The backend used it to find trusted session data."));
});

// WHAT:
// Logout removes the backend session values.
//
// WHY:
// The browser can delete cookies, but the trusted logout action is clearing
// server-side session state so the cookie no longer maps to a logged-in user.
app.MapPost("/api/logout", (HttpContext httpContext) =>
{
    httpContext.Session.Clear();

    return Results.Ok(new SessionStatusResponse(
        IsLoggedIn: false,
        UserName: null,
        DisplayName: null,
        Explanation: "The backend session was cleared. The browser no longer has a logged-in identity."));
});

// WHAT:
// This endpoint returns the authoritative records from the SQLite database.
//
// WHY:
// Later demos can store copies of this data in localStorage, IndexedDB, Cache
// API, HTTP cache, and IMemoryCache. Those copies may be useful, but this
// endpoint represents the backend source of truth.
app.MapGet("/api/authoritative-data", async (
    LabDataStore dataStore,
    CancellationToken cancellationToken) =>
{
    var records = await dataStore.GetRecordsAsync(cancellationToken);
    return Results.Ok(records);
});

// WHAT:
// This endpoint creates or updates one authoritative database record.
//
// WHY:
// Cache lessons need a way to change the real backend value so stale frontend
// or cached copies can be compared against it.
app.MapPost("/api/authoritative-data", async (
    SaveLabDataRecordRequest request,
    LabDataStore dataStore,
    CancellationToken cancellationToken) =>
{
    var record = await dataStore.UpsertRecordAsync(
        request.Name,
        request.Value,
        cancellationToken);

    return Results.Ok(record);
});

app.MapFallbackToFile("index.html");

app.Run();

public sealed record SaveLabDataRecordRequest(string Name, string Value);

public sealed record LoginRequest(string UserName, string Password);

public sealed record SessionStatusResponse(
    bool IsLoggedIn,
    string? UserName,
    string? DisplayName,
    string Explanation);
