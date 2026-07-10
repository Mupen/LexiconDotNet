using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using System.Data;
using ReactNews.Application.Interfaces;
using ReactNews.Infrastructure.Caching.NewsProviders;
using ReactNews.Infrastructure.Options.NewsApi;
using ReactNews.Infrastructure.Persistence;
using ReactNews.Infrastructure.Providers;
using ReactNews.Infrastructure.Providers.NewsApi;
using ReactNews.Infrastructure.Storage.ArticleSnapshots;
using ReactNews.Infrastructure.Storage.EditorialArticles;
using ReactNews.Infrastructure.Storage.ReaderPreferences;
using ReactNews.Infrastructure.Storage.SavedArticles;
using ReactNews.Infrastructure.Storage.Users;

namespace ReactNews.Infrastructure;

/// <summary>
/// Registers infrastructure implementations for application interfaces.
/// </summary>
/// <remarks>
/// What: this method wires NewsAPI, memory caching, and article snapshot storage.
/// Why: the API project should not know which concrete class calls NewsAPI or
/// which storage mechanism keeps article snapshots. That is infrastructure's job.
/// How: Application defines interfaces, Infrastructure implements them, and this
/// extension connects those implementations to dependency injection.
/// </remarks>
public static class InfrastructureDependencyInjection
{
    private const string InitialMigrationId = "20260710060354_InitialCreate";
    private const string EfProductVersion = "10.0.9";

    /// <summary>
    /// Registers concrete infrastructure services.
    /// </summary>
    /// <remarks>
    /// What: connects EF Core, SQLite, memory cache, snapshot storage, NewsAPI
    /// HTTP client, and cache-wrapped news feed client to dependency injection.
    /// How: Application interfaces are mapped to Infrastructure implementations,
    /// and NewsApiClient is registered as the raw source behind CachedNewsFeedClient.
    /// Why: Program.cs should compose the app but not know detailed
    /// implementation classes for storage, caching, or external HTTP calls.
    /// </remarks>
    public static IServiceCollection AddReactNewsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ReactNews")
            ?? "Data Source=Data/reactnews.db";

        services.AddDbContext<ReactNewsDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddMemoryCache();
        services.AddScoped<IArticleSnapshotStore, EfArticleSnapshotStore>();
        services.AddScoped<IEditorialArticleStore, EfEditorialArticleStore>();
        services.AddScoped<IReaderPreferencesStore, EfReaderPreferencesStore>();
        services.AddScoped<ISavedArticleStore, EfSavedArticleStore>();
        services.AddScoped<IUserStore, EfUserStore>();
        services.AddScoped<INewsProvider, CachedNewsFeedClient>();
        services.Configure<NewsApiOptions>(options => { });

        services.AddHttpClient<NewsApiClient>((serviceProvider, client) =>
        {
            /*
             * What: configure the HttpClient used by NewsApiClient.
             * How: the base URL comes from NewsApiOptions and a User-Agent header
             * is added to identify the ReactNews application.
             * Why: keeping this setup in DI avoids hard-coding runtime
             * configuration inside NewsApiClient and makes tests able to create
             * their own HttpClient.
             */
            var options = serviceProvider
                .GetRequiredService<IOptions<NewsApiOptions>>()
                .Value;

            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("ReactNews/1.0");
        });

        services.AddScoped<INewsProviderSource>(serviceProvider =>
            serviceProvider.GetRequiredService<NewsApiClient>());

        return services;
    }

    /// <summary>
    /// Applies ReactNews database migrations when needed.
    /// </summary>
    /// <remarks>
    /// What: upgrades the SQLite database to the newest EF Core migration.
    /// How: it creates a service scope, resolves ReactNewsDbContext, and calls
    /// Database.Migrate so EF creates the database and applies pending migration
    /// files in order.
    /// Why: migrations are safer than ad-hoc startup SQL because schema changes
    /// become named, reviewable files that can be applied consistently by
    /// Visual Studio, Start.ps1, Docker, and tests.
    /// </remarks>
    public static void MigrateReactNewsDatabase(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ReactNewsDbContext>();

        MarkInitialMigrationAppliedForLegacyDatabase(dbContext);
        dbContext.Database.Migrate();
    }

    /// <summary>
    /// Marks the first migration as applied for databases created before migrations existed.
    /// </summary>
    /// <remarks>
    /// What: detects an older ReactNews SQLite database that already has the
    /// application tables but does not have EF Core's __EFMigrationsHistory table.
    /// How: it checks for ArticleSnapshots as the original persistence table,
    /// creates __EFMigrationsHistory, and inserts the InitialCreate migration id.
    /// Why: earlier database files can have the correct tables but no migration
    /// history. Without this transition, Database.Migrate would try to create
    /// tables that already exist.
    /// </remarks>
    private static void MarkInitialMigrationAppliedForLegacyDatabase(ReactNewsDbContext dbContext)
    {
        var connection = dbContext.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
        }

        if (!TableExists(connection, "ArticleSnapshots") || TableExists(connection, "__EFMigrationsHistory"))
        {
            return;
        }

        using var createHistoryCommand = connection.CreateCommand();
        createHistoryCommand.CommandText = """
            CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
                "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
                "ProductVersion" TEXT NOT NULL
            );
            """;
        createHistoryCommand.ExecuteNonQuery();

        using var insertHistoryCommand = connection.CreateCommand();
        insertHistoryCommand.CommandText = """
            INSERT OR IGNORE INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            VALUES ($migrationId, $productVersion);
            """;

        var migrationIdParameter = insertHistoryCommand.CreateParameter();
        migrationIdParameter.ParameterName = "$migrationId";
        migrationIdParameter.Value = InitialMigrationId;
        insertHistoryCommand.Parameters.Add(migrationIdParameter);

        var productVersionParameter = insertHistoryCommand.CreateParameter();
        productVersionParameter.ParameterName = "$productVersion";
        productVersionParameter.Value = EfProductVersion;
        insertHistoryCommand.Parameters.Add(productVersionParameter);

        insertHistoryCommand.ExecuteNonQuery();
    }

    /// <summary>
    /// Checks whether a SQLite table exists.
    /// </summary>
    /// <remarks>
    /// What: answers whether a named table is present in the current database.
    /// How: queries sqlite_master with a parameterized table name.
    /// Why: the legacy migration bridge needs table existence checks without
    /// depending on EF metadata, because the problem is specifically missing EF
    /// migration metadata.
    /// </remarks>
    private static bool TableExists(IDbConnection connection, string tableName)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = $tableName;";

        var parameter = command.CreateParameter();
        parameter.ParameterName = "$tableName";
        parameter.Value = tableName;
        command.Parameters.Add(parameter);

        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }
}
