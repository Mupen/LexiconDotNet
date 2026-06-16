namespace CleanBookingV2.Application.Requests.Bookings;

/// <summary>
/// Carries create-booking input from the API layer into the application use case.
/// This request type keeps the use case independent from MVC DTO attributes and
/// HTTP-specific concerns.
/// </summary>
public sealed record CreateBookingRequest(
    string GuestName,
    DateTime CheckIn,
    DateTime CheckOut,
    int NumberOfGuests,
    int RoomId,
    int? ParkingSpaceId,
    TimeOnly? EstimatedArrivalTime);
