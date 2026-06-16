using CleanBookingV2.Domain.Entities;

namespace CleanBookingV2.Application.Interfaces;

/// <summary>
/// Provides write-oriented access to Booking domain entities.
/// Use cases need tracked entities when they create, update, or cancel bookings.
/// Read-only API projections use IBookingReadRepository instead so this interface
/// stays focused on domain mutations.
/// </summary>
public interface IBookingRepository
{
    Task<IReadOnlyList<Booking>> GetAllAsync(CancellationToken cancellationToken);
    Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Booking>> GetByRoomIdAsync(int roomId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Booking>> GetByParkingSpaceIdAsync(int parkingSpaceId, CancellationToken cancellationToken);
    Task AddAsync(Booking booking, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
