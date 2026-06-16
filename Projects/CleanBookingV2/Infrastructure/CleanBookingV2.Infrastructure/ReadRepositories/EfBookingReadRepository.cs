using CleanBookingV2.Application.Interfaces;
using CleanBookingV2.Application.ReadModels;
using CleanBookingV2.Domain.Entities;
using CleanBookingV2.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CleanBookingV2.Infrastructure.ReadRepositories;

/// <summary>
/// EF Core read repository for booking API projections.
/// It joins bookings with room and parking names in one query so controllers do not
/// need to stitch together multiple repositories or expose domain entities.
/// </summary>
public sealed class EfBookingReadRepository : IBookingReadRepository
{
    private readonly CleanBookingV2DbContext _dbContext;

    public EfBookingReadRepository(CleanBookingV2DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Returns all bookings as read models ordered by check-in date.
    /// AsNoTracking is used because these results are only displayed, never edited
    /// through the returned objects.
    /// </summary>
    public async Task<IReadOnlyList<BookingReadModel>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await CreateBookingReadModelQuery(
                _dbContext.Bookings
                    .AsNoTracking()
                    .OrderBy(booking => booking.Stay.Start))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Returns one booking read model or null when it does not exist.
    /// Reusing the same projection method keeps single-item and list responses
    /// structurally consistent.
    /// </summary>
    public async Task<BookingReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await CreateBookingReadModelQuery(_dbContext.Bookings.AsNoTracking().Where(booking => booking.Id == id))
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Builds the shared LINQ projection for booking read models.
    /// The left join to parking allows bookings without parking to still appear in
    /// results. Keeping this as IQueryable lets EF Core translate the whole shape
    /// into SQL instead of materializing entities first.
    /// </summary>
    private IQueryable<BookingReadModel> CreateBookingReadModelQuery(IQueryable<Booking> bookings)
    {
        return
            from booking in bookings
            join room in _dbContext.Rooms.AsNoTracking()
                on booking.RoomId equals room.Id
            join parkingSpace in _dbContext.ParkingSpaces.AsNoTracking()
                on booking.ParkingSpaceId equals (int?)parkingSpace.Id into parkingSpaces
            from parkingSpace in parkingSpaces.DefaultIfEmpty()
            select new BookingReadModel(
                booking.Id,
                booking.GuestName,
                booking.Stay.Start,
                booking.Stay.End,
                booking.NumberOfGuests,
                booking.RoomId,
                room.Name,
                booking.ParkingSpaceId,
                parkingSpace == null ? null : parkingSpace.Name,
                booking.TotalPrice,
                booking.EstimatedArrivalTime,
                booking.Status);
    }
}
