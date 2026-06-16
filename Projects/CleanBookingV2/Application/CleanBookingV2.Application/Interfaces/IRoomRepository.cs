using CleanBookingV2.Domain.Entities;

namespace CleanBookingV2.Application.Interfaces;

/// <summary>
/// Provides access to room domain entities.
/// Booking preparation uses rooms as authoritative backend data for capacity and
/// price, rather than trusting values from the frontend.
/// </summary>
public interface IRoomRepository
{
    Task<IReadOnlyList<Room>> GetAllAsync(CancellationToken cancellationToken);
    Task<Room?> GetByIdAsync(int id, CancellationToken cancellationToken);
}
