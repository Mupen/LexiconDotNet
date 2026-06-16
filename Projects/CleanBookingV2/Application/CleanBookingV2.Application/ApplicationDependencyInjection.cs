using CleanBookingV2.Application.Queries.Bookings;
using CleanBookingV2.Application.Queries.Parking;
using CleanBookingV2.Application.Queries.Rooms;
using CleanBookingV2.Application.Services;
using CleanBookingV2.Application.UseCases.Bookings;
using Microsoft.Extensions.DependencyInjection;

namespace CleanBookingV2.Application;

/// <summary>
/// Registers application-layer services with dependency injection.
/// Keeping these registrations in the Application project makes Program.cs smaller
/// and keeps use case wiring close to the classes being wired. Infrastructure
/// registrations stay separate because they depend on EF Core and SQLite.
/// </summary>
public static class ApplicationDependencyInjection
{
    /// <summary>
    /// Adds use cases, queries, and application services.
    /// Scoped lifetime is used because these services participate in a single web
    /// request and often depend on scoped infrastructure services such as DbContext.
    /// </summary>
    public static IServiceCollection AddCleanBookingApplication(this IServiceCollection services)
    {
        services.AddScoped<BookingAvailabilityService>();
        services.AddScoped<BookingPreparationService>();
        services.AddScoped<CreateBooking>();
        services.AddScoped<UpdateBooking>();
        services.AddScoped<CancelBooking>();
        services.AddScoped<GetAllBookings>();
        services.AddScoped<GetBookingById>();
        services.AddScoped<GetAllRooms>();
        services.AddScoped<GetAvailableRooms>();
        services.AddScoped<GetAllParkingSpaces>();
        services.AddScoped<GetAvailableParkingSpaces>();

        return services;
    }
}
