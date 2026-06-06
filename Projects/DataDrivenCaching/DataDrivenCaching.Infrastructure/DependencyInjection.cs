using DataDrivenCaching.Infrastructure.DataStores;
using DataDrivenCaching.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DataDrivenCaching.Infrastructure;

// WHAT:
// This extension method registers the Infrastructure layer.
//
// WHY:
// Program.cs should say "use DataDrivenCaching infrastructure" instead of
// knowing every database and repository type. That keeps startup readable while
// still making the physical storage decision explicit.
public static class DependencyInjection
{
    public static IServiceCollection AddDataDrivenCachingInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        // WHAT:
        // UseSqlite tells EF Core to store authoritative backend data in a
        // SQLite file.
        //
        // WHY:
        // SQLite is simple enough for a learning project but still real durable
        // storage. This lets demos compare database data against frontend
        // storage, HTTP cache, service worker cache, and backend memory cache.
        services.AddDbContext<DataDrivenCachingDbContext>(options =>
            options.UseSqlite(connectionString));

        // WHAT:
        // Register the concrete data store directly.
        //
        // WHY:
        // Data-driven design should not hide a single real storage choice behind
        // an interface by default. If the project later adds a real Redis-backed
        // or file-backed store, then an interface may become useful.
        services.AddScoped<LabDataStore>();
        services.AddScoped<LabUserStore>();

        return services;
    }
}
