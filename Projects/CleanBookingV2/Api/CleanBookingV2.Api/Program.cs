using CleanBookingV2.Application;
using CleanBookingV2.Infrastructure;
using CleanBookingV2.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Use explicit logging providers so local development output is predictable.
// Clearing defaults avoids hidden provider differences between templates or hosts,
// then console/debug are added because they are useful for a learning project.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// The API owns the authoritative database connection. Failing fast on a missing
// connection string is clearer than starting the app and failing later on the first
// request that touches persistence.
var connectionString = builder.Configuration.GetConnectionString("CleanBookingV2")
    ?? throw new InvalidOperationException("Connection string 'CleanBookingV2' is missing.");

// SQLite stores the local database file under Data. Creating the folder at startup
// keeps local and Docker runs simple without requiring a manual setup step.
Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, "Data"));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Enums are serialized as strings so API responses are readable and stable
        // for the React frontend. Numeric enum values would be harder to understand
        // and easier to break if enum ordering changed.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        // CORS is restricted to local frontend development origins. The API is still
        // a demo app without authentication, so keeping origins explicit is better
        // than AllowAnyOrigin.
        policy
            .WithOrigins(
                "http://localhost:5173",
                "http://127.0.0.1:5173",
                "http://localhost:3000",
                "http://127.0.0.1:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Register application services before infrastructure so use cases can be resolved
// with concrete repository implementations supplied by the Infrastructure project.
builder.Services.AddCleanBookingApplication();
builder.Services.AddCleanBookingInfrastructure(connectionString);

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    // Swagger is enabled only for development because it is a manual exploration
    // tool, not part of the production booking workflow.
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapGet("/", () => Results.Redirect("/swagger"));
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CleanBookingV2DbContext>();
    // Local/demo deployment convenience: apply migrations on startup.
    // For production, generate and review SQL migration scripts in CI/CD instead
    // of letting every app instance mutate the schema at runtime.
    await dbContext.Database.MigrateAsync();
}

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthorization();
app.MapControllers();
app.Run();
