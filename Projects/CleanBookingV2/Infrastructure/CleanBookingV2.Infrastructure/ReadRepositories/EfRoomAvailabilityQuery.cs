using CleanBookingV2.Application.Interfaces;
using CleanBookingV2.Application.ReadModels;
using CleanBookingV2.Domain.Enums;
using CleanBookingV2.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CleanBookingV2.Infrastructure.ReadRepositories;

/// <summary>
/// EF Core implementation of the available-room query.
/// This query is read-optimized: it projects directly to RoomReadModel and lets the
/// database filter out conflicting bookings instead of loading all rooms/bookings
/// into application memory.
/// </summary>
public sealed class EfRoomAvailabilityQuery : IRoomAvailabilityQuery
{
    private readonly CleanBookingV2DbContext _dbContext;

    public EfRoomAvailabilityQuery(CleanBookingV2DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Returns active rooms with enough capacity and no active overlapping booking.
    /// The conflicting-room subquery translates the same half-open overlap rule used
    /// by the domain into SQL. Create/update still re-checks availability because
    /// frontend search results are only snapshots.
    /// </summary>
    public async Task<IReadOnlyList<RoomReadModel>> GetAvailableRoomsAsync(
        DateTime checkIn,
        DateTime checkOut,
        int numberOfGuests,
        CancellationToken cancellationToken)
    {
        // This translates the overlap check into SQL and avoids one booking query
        // per room. SQLite has no range exclusion constraint, so this project keeps
        // the authoritative re-check in the application layer and uses transactions
        // plus concurrency tokens for updates.
        var conflictingRoomIds = _dbContext.Bookings
            .AsNoTracking()
            .Where(booking =>
                booking.Status == BookingStatus.Active &&
                booking.Stay.Start < checkOut &&
                checkIn < booking.Stay.End)
            .Select(booking => booking.RoomId);

        return await _dbContext.Rooms
            .AsNoTracking()
            .Where(room =>
                room.IsActive &&
                room.Capacity >= numberOfGuests &&
                !conflictingRoomIds.Contains(room.Id))
            .OrderBy(room => room.Id)
            .Select(room => new RoomReadModel(
                room.Id,
                room.Name,
                room.RoomType,
                room.SizeInSquareMeters,
                room.Capacity,
                room.PricePerNight,
                room.IsActive))
            .ToListAsync(cancellationToken);
    }
}
