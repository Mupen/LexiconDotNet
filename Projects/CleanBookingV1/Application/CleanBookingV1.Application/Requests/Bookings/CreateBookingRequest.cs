using CleanBookingV1.Domain.ValueObjects;

namespace CleanBookingV1.Application.Requests.Bookings;

public sealed record CreateBookingRequest(
    string GuestName,
    int RoomId,
    DateTime CheckIn,
    DateTime CheckOut,
    int NumberOfGuests,
    bool ParkingRequested,
    TimeOnly? EstimatedArrivalTime);