using CleanBookingV2.Domain.Enums;

namespace CleanBookingV2.Api.Contracts.Bookings;

/// <summary>
/// Public response shape for booking reads.
/// This is separate from the domain Booking entity so the API can include display
/// fields such as room name and parking name without changing the domain model.
/// </summary>
public sealed record BookingResponse(
    Guid Id,
    string GuestName,
    DateTime CheckIn,
    DateTime CheckOut,
    int NumberOfGuests,
    int RoomId,
    string RoomName,
    int? ParkingSpaceId,
    string? ParkingSpaceName,
    decimal TotalPrice,
    TimeOnly? EstimatedArrivalTime,
    BookingStatus Status);
