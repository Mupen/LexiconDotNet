using CleanBookingV2.Api.Contracts.Bookings;
using CleanBookingV2.Api.Contracts.Parking;
using CleanBookingV2.Api.Contracts.Rooms;
using CleanBookingV2.Application.ReadModels;

namespace CleanBookingV2.Api.Mapping;

/// <summary>
/// Converts application read models into API response records.
/// This keeps controller actions small and gives one place to adjust the public
/// response shape if the internal read models change.
/// </summary>
public static class ResponseMapping
{
    /// <summary>
    /// Maps a booking read model to its HTTP response contract.
    /// The response includes display names because the read model already performed
    /// the required joins in infrastructure.
    /// </summary>
    public static BookingResponse ToResponse(this BookingReadModel booking)
    {
        return new BookingResponse(
            booking.Id,
            booking.GuestName,
            booking.CheckIn,
            booking.CheckOut,
            booking.NumberOfGuests,
            booking.RoomId,
            booking.RoomName,
            booking.ParkingSpaceId,
            booking.ParkingSpaceName,
            booking.TotalPrice,
            booking.EstimatedArrivalTime,
            booking.Status);
    }

    /// <summary>
    /// Maps a room read model to its HTTP response contract.
    /// Keeping this mapping explicit avoids leaking future internal-only fields.
    /// </summary>
    public static RoomResponse ToResponse(this RoomReadModel room)
    {
        return new RoomResponse(
            room.Id,
            room.Name,
            room.RoomType,
            room.SizeInSquareMeters,
            room.Capacity,
            room.PricePerNight,
            room.IsActive);
    }

    /// <summary>
    /// Maps a parking space read model to its HTTP response contract.
    /// The response record mirrors only the fields the frontend needs.
    /// </summary>
    public static ParkingSpaceResponse ToResponse(this ParkingSpaceReadModel space)
    {
        return new ParkingSpaceResponse(
            space.Id,
            space.Name,
            space.ParkingSpaceType,
            space.IsActive);
    }
}
