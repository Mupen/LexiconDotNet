using CleanBookingV2.Application.Interfaces;
using CleanBookingV2.Application.ReadModels;
using CleanBookingV2.Domain.Enums;
using CleanBookingV2.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CleanBookingV2.Infrastructure.ReadRepositories;

/// <summary>
/// EF Core implementation of the available-parking query.
/// Parking availability is separate from room availability because a booking may
/// have no parking or may need a different parking resource timeline.
/// </summary>
public sealed class EfParkingSpaceAvailabilityQuery : IParkingSpaceAvailabilityQuery
{
    private readonly CleanBookingV2DbContext _dbContext;

    public EfParkingSpaceAvailabilityQuery(CleanBookingV2DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Returns active parking spaces with no active overlapping booking.
    /// The query filters at the database level for efficiency and projects directly
    /// into read models for API output.
    /// </summary>
    public async Task<IReadOnlyList<ParkingSpaceReadModel>> GetAvailableParkingSpacesAsync(
        DateTime checkIn,
        DateTime checkOut,
        CancellationToken cancellationToken)
    {
        var conflictingParkingSpaceIds = _dbContext.Bookings
            .AsNoTracking()
            .Where(booking =>
                booking.Status == BookingStatus.Active &&
                booking.ParkingSpaceId != null &&
                booking.Stay.Start < checkOut &&
                checkIn < booking.Stay.End)
            .Select(booking => booking.ParkingSpaceId!.Value);

        return await _dbContext.ParkingSpaces
            .AsNoTracking()
            .Where(space =>
                space.IsActive &&
                !conflictingParkingSpaceIds.Contains(space.Id))
            .OrderBy(space => space.Id)
            .Select(space => new ParkingSpaceReadModel(
                space.Id,
                space.Name,
                space.ParkingSpaceType,
                space.IsActive))
            .ToListAsync(cancellationToken);
    }
}
