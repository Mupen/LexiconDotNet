using CleanBookingV2.Application.Interfaces;
using CleanBookingV2.Infrastructure.Persistence;
using CleanBookingV2.Infrastructure.ReadRepositories;
using CleanBookingV2.Infrastructure.Repositories;
using CleanBookingV2.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CleanBookingV2.Infrastructure;

/// <summary>
/// Registers infrastructure implementations for application interfaces.
/// This keeps Program.cs from knowing every repository type and keeps EF/SQLite
/// dependencies isolated in the Infrastructure project.
/// </summary>
public static class InfrastructureDependencyInjection
{
    /// <summary>
    /// Adds SQLite persistence, repositories, read queries, transactions, and system services.
    /// The connection string is supplied by the API project so infrastructure stays
    /// reusable across local, Docker, or test configurations.
    /// </summary>
    public static IServiceCollection AddCleanBookingInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        // DbContext is scoped because each web request should get one unit of work.
        // EF Core then tracks changes consistently within that request.
        services.AddDbContext<CleanBookingV2DbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IBookingRepository, EfBookingRepository>();
        services.AddScoped<IBookingReadRepository, EfBookingReadRepository>();
        services.AddScoped<IBookingTransaction, EfBookingTransaction>();
        services.AddScoped<IRoomAvailabilityQuery, EfRoomAvailabilityQuery>();
        services.AddScoped<IParkingSpaceAvailabilityQuery, EfParkingSpaceAvailabilityQuery>();
        services.AddScoped<IRoomRepository, EfRoomRepository>();
        services.AddScoped<IParkingSpaceRepository, EfParkingSpaceRepository>();
        services.AddSingleton<IGuidGenerator, SystemGuidGenerator>();

        return services;
    }
}
