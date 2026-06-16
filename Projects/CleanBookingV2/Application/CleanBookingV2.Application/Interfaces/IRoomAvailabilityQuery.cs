using CleanBookingV2.Application.ReadModels;

namespace CleanBookingV2.Application.Interfaces;

/// <summary>
/// Provides an optimized read query for available rooms.
/// The implementation can translate overlap checks to SQL while the application
/// layer depends only on the business-shaped query contract.
/// </summary>
public interface IRoomAvailabilityQuery
{
    Task<IReadOnlyList<RoomReadModel>> GetAvailableRoomsAsync(
        DateTime checkIn,
        DateTime checkOut,
        int numberOfGuests,
        CancellationToken cancellationToken);
}
