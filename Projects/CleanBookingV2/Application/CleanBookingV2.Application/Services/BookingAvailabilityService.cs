using CleanBookingV2.Application.Interfaces;
using CleanBookingV2.Domain.Contracts;
using CleanBookingV2.Domain.Entities;
using CleanBookingV2.Domain.ValueObjects;

namespace CleanBookingV2.Application.Services;

/// <summary>
/// Performs availability checks for rooms and parking spaces.
/// This service lives in Application because availability is a workflow rule that
/// needs repositories, not just data already inside one entity. It asks domain
/// entities whether they overlap so the actual overlap meaning still stays in the
/// domain model.
/// </summary>
public sealed class BookingAvailabilityService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IParkingSpaceRepository _parkingSpaceRepository;

    public BookingAvailabilityService(
        IBookingRepository bookingRepository,
        IParkingSpaceRepository parkingSpaceRepository)
    {
        _bookingRepository = bookingRepository;
        _parkingSpaceRepository = parkingSpaceRepository;
    }

    /// <summary>
    /// Ensures that no active overlapping booking already uses the selected room.
    /// The optional booking id is ignored during updates so a booking can keep its
    /// own room and dates without being treated as a conflict with itself.
    /// </summary>
    public async Task<Result> EnsureRoomAvailableAsync(
        int roomId,
        DateRange stay,
        Guid? bookingIdToIgnore,
        CancellationToken cancellationToken)
    {
        var roomBookings = await _bookingRepository.GetByRoomIdAsync(roomId, cancellationToken);

        bool unavailable = roomBookings.Any(booking =>
            booking.Id != bookingIdToIgnore &&
            booking.OverlapsRoom(roomId, stay));

        return unavailable
            ? Result.Failure(new Error("Booking.RoomUnavailable", "The selected room is not available for the requested stay."))
            : Result.Success();
    }

    /// <summary>
    /// Ensures that an optional parking space exists, is active, and is not already booked.
    /// Parking is optional, so a null parking id succeeds immediately. This keeps
    /// the booking workflow simple: callers can always ask this method to validate
    /// parking without branching first.
    /// </summary>
    public async Task<Result> EnsureParkingAvailableAsync(
        int? parkingSpaceId,
        DateRange stay,
        Guid? bookingIdToIgnore,
        CancellationToken cancellationToken)
    {
        if (parkingSpaceId is null)
            return Result.Success();

        ParkingSpace? space = await _parkingSpaceRepository.GetByIdAsync(parkingSpaceId.Value, cancellationToken);
        if (space is null)
            return Result.Failure(new Error("Booking.ParkingNotFound", "The selected parking space was not found."));

        if (!space.IsActive)
            return Result.Failure(new Error("Booking.ParkingInactive", "The selected parking space is inactive."));

        var bookings = await _bookingRepository.GetByParkingSpaceIdAsync(space.Id, cancellationToken);
        bool unavailable = bookings.Any(booking =>
            booking.Id != bookingIdToIgnore &&
            booking.OverlapsParkingSpace(space.Id, stay));

        return unavailable
            ? Result.Failure(new Error("Booking.ParkingUnavailable", "The selected parking space is not available for the requested stay."))
            : Result.Success();
    }
}
