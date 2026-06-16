using CleanBookingV2.Domain.Entities;

namespace CleanBookingV2.Application.Interfaces;

/// <summary>
/// Provides access to parking space domain entities.
/// Booking preparation needs this to verify that a selected parking space exists
/// and is active before assigning it to a booking.
/// </summary>
public interface IParkingSpaceRepository
{
    Task<IReadOnlyList<ParkingSpace>> GetAllAsync(CancellationToken cancellationToken);
    Task<ParkingSpace?> GetByIdAsync(int id, CancellationToken cancellationToken);
}
