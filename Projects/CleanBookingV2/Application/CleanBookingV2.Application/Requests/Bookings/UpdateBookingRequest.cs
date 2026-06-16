namespace CleanBookingV2.Application.Requests.Bookings;

/// <summary>
/// Carries update-booking input from the API layer into the application use case.
/// The id is included in the request object so the use case receives one complete
/// command instead of mixing route values and body values.
/// </summary>
public sealed record UpdateBookingRequest(
    Guid Id,
    string GuestName,
    DateTime CheckIn,
    DateTime CheckOut,
    int NumberOfGuests,
    int RoomId,
    int? ParkingSpaceId,
    TimeOnly? EstimatedArrivalTime);
