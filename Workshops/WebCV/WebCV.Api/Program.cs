using Microsoft.EntityFrameworkCore;
using WebCV.Application;
using WebCV.Application.Queries;
using WebCV.Application.UseCases;
using WebCV.Infrastructure;
using WebCV.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var connectionString = builder.Configuration.GetConnectionString("WebCV")
    ?? throw new InvalidOperationException("Connection string 'WebCV' is missing.");

var seedDataPath = builder.Configuration["SeedData:DefaultCvPath"]
    ?? throw new InvalidOperationException("Seed data path 'SeedData:DefaultCvPath' is missing.");

Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, "Data"));

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddWebCvApplication();
builder.Services.AddWebCvInfrastructure(connectionString);

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<WebCvDbContext>();
    var seedFilePath = Path.Combine(builder.Environment.ContentRootPath, seedDataPath);

    await dbContext.Database.EnsureCreatedAsync();

    // The first run creates the local profile from JSON; after that the database is the source of truth.
    await WebCvSeedData.SeedDevelopmentDataAsync(dbContext, seedFilePath);
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/cv", async (
    GetDefaultCvProfile query,
    CancellationToken cancellationToken) =>
{
    var profile = await query.ExecuteAsync(cancellationToken);
    return profile is null ? Results.NotFound() : Results.Ok(profile);
})
.WithName("GetDefaultCv");

app.MapPut("/api/cv", async (
    ReplaceCvProfileRequest request,
    ReplaceDefaultCvProfile useCase,
    GetDefaultCvProfile query,
    CancellationToken cancellationToken) =>
{
    await useCase.ExecuteAsync(request, cancellationToken);
    var profile = await query.ExecuteAsync(cancellationToken);
    return Results.Ok(profile);
})
.WithName("ReplaceDefaultCv");

app.MapFallbackToFile("index.html");

app.Run();
