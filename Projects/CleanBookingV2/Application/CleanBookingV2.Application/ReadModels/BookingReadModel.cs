using CleanBookingV2.Domain.Enums;

namespace CleanBookingV2.Application.ReadModels;

/// <summary>
/// Represents booking data shaped for API reads.
/// Read models are intentionally separate from domain entities because API screens
/// need joined display information such as room and parking names, while domain
/// entities focus on business behavior and persistence identity.
/// </summary>
public sealed record BookingReadModel(
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
