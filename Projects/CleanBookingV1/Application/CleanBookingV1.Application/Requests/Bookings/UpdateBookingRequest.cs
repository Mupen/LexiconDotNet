using CleanBookingV1.Domain.ValueObjects;

namespace CleanBookingV1.Application.UseCases.Bookings;

public sealed record UpdateBookingRequest(
    Guid BookingId,
    string GuestName,
    int RoomId,
    DateTime CheckIn,
    DateTime CheckOut,
    int NumberOfGuests,
    bool ParkingRequested,
    TimeOnly? EstimatedArrivalTime);