using CleanBookingV2.Application.Interfaces;
using CleanBookingV2.Application.ReadModels;
using CleanBookingV2.Domain.ValueObjects;

namespace CleanBookingV2.Application.Queries.Parking;

/// <summary>
/// Reads parking spaces available for a requested stay.
/// It validates the requested stay with the domain DateRange before asking
/// infrastructure to perform the efficient database-side availability query.
/// </summary>
public sealed class GetAvailableParkingSpaces
{
    private readonly IParkingSpaceAvailabilityQuery _parkingSpaceAvailabilityQuery;

    public GetAvailableParkingSpaces(IParkingSpaceAvailabilityQuery parkingSpaceAvailabilityQuery)
    {
        _parkingSpaceAvailabilityQuery = parkingSpaceAvailabilityQuery;
    }

    /// <summary>
    /// Returns parking spaces that do not have active overlapping bookings.
    /// This is a read-only snapshot for the frontend; create/update still re-checks
    /// availability before saving because frontend state can become stale.
    /// </summary>
    public async Task<IReadOnlyList<ParkingSpaceReadModel>> ExecuteAsync(
        DateTime checkIn,
        DateTime checkOut,
        CancellationToken cancellationToken)
    {
        _ = new DateRange(checkIn, checkOut);
        return await _parkingSpaceAvailabilityQuery.GetAvailableParkingSpacesAsync(checkIn, checkOut, cancellationToken);
    }
}
