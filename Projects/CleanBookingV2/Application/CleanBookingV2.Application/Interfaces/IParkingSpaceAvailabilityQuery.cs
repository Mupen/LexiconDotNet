using CleanBookingV2.Application.ReadModels;

namespace CleanBookingV2.Application.Interfaces;

/// <summary>
/// Provides an optimized read query for available parking spaces.
/// This is kept separate from repositories because availability reads can be
/// expressed more efficiently as database projections than by loading every entity.
/// </summary>
public interface IParkingSpaceAvailabilityQuery
{
    Task<IReadOnlyList<ParkingSpaceReadModel>> GetAvailableParkingSpacesAsync(
        DateTime checkIn,
        DateTime checkOut,
        CancellationToken cancellationToken);
}
