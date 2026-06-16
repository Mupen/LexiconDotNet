using CleanBookingV2.Application.Interfaces;
using CleanBookingV2.Application.ReadModels;
using CleanBookingV2.Domain.ValueObjects;

namespace CleanBookingV2.Application.Queries.Rooms;

/// <summary>
/// Reads rooms available for a requested stay and guest count.
/// The application layer validates the date range before delegating to an optimized
/// infrastructure query, keeping business validation separate from SQL details.
/// </summary>
public sealed class GetAvailableRooms
{
    private readonly IRoomAvailabilityQuery _roomAvailabilityQuery;

    public GetAvailableRooms(IRoomAvailabilityQuery roomAvailabilityQuery)
    {
        _roomAvailabilityQuery = roomAvailabilityQuery;
    }

    /// <summary>
    /// Returns rooms that can accept the requested dates and guest count.
    /// Creating a DateRange here reuses domain validation even though this is a read
    /// query, so availability searches and booking saves agree on date validity.
    /// </summary>
    public async Task<IReadOnlyList<RoomReadModel>> ExecuteAsync(
        DateTime checkIn,
        DateTime checkOut,
        int numberOfGuests,
        CancellationToken cancellationToken)
    {
        _ = new DateRange(checkIn, checkOut);
        return await _roomAvailabilityQuery.GetAvailableRoomsAsync(checkIn, checkOut, numberOfGuests, cancellationToken);
    }
}
