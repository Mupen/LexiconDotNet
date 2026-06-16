using CleanBookingV2.Application.Interfaces;
using CleanBookingV2.Domain.Contracts;
using CleanBookingV2.Domain.ValueObjects;

namespace CleanBookingV2.Application.Services;

/// <summary>
/// Prepares all derived data needed before creating or updating a booking.
/// This service centralizes the shared workflow for create and update: load room,
/// validate capacity, create the stay, check availability, and calculate price.
/// Keeping that sequence here avoids duplicating it in both use cases.
/// </summary>
public sealed class BookingPreparationService
{
    private readonly IRoomRepository _roomRepository;
    private readonly BookingAvailabilityService _availabilityService;

    public BookingPreparationService(
        IRoomRepository roomRepository,
        BookingAvailabilityService availabilityService)
    {
        _roomRepository = roomRepository;
        _availabilityService = availabilityService;
    }

    /// <summary>
    /// Builds a BookingPreparation object when the request is valid.
    /// The method returns Result instead of throwing for expected business failures
    /// so controllers can show clear problem details without exception-driven flow.
    /// </summary>
    public async Task<Result<BookingPreparation>> PrepareAsync(
        DateTime checkIn,
        DateTime checkOut,
        int numberOfGuests,
        int roomId,
        int? parkingSpaceId,
        Guid? bookingIdToIgnore,
        CancellationToken cancellationToken)
    {
        // Load the authoritative room from the backend store. The frontend may send
        // a room id from its working state, but the backend must re-check existence,
        // active status, capacity, and price before saving.
        var room = await _roomRepository.GetByIdAsync(roomId, cancellationToken);
        if (room is null)
            return Result<BookingPreparation>.Failure(new Error("Booking.RoomNotFound", "The selected room was not found."));

        if (!room.IsActive)
            return Result<BookingPreparation>.Failure(new Error("Booking.RoomInactive", "The selected room is inactive."));

        if (numberOfGuests > room.Capacity)
            return Result<BookingPreparation>.Failure(new Error("Booking.RoomCapacityExceeded", $"Room '{room.Name}' allows a maximum of {room.Capacity} guest(s)."));

        // DateRange construction validates date ordering once and then lets the
        // rest of the workflow use a trusted value object.
        Result<DateRange> stayResult = CreateStay(checkIn, checkOut);
        if (stayResult.IsFailure)
            return Result<BookingPreparation>.Failure(stayResult.Error!);

        DateRange stay = stayResult.Value!;
        Result roomAvailability = await _availabilityService.EnsureRoomAvailableAsync(room.Id, stay, bookingIdToIgnore, cancellationToken);
        if (roomAvailability.IsFailure)
            return Result<BookingPreparation>.Failure(roomAvailability.Error!);

        Result parkingAvailability = await _availabilityService.EnsureParkingAvailableAsync(parkingSpaceId, stay, bookingIdToIgnore, cancellationToken);
        if (parkingAvailability.IsFailure)
            return Result<BookingPreparation>.Failure(parkingAvailability.Error!);

        // Price is calculated from backend-owned room data. The frontend can show
        // estimates, but the saved total must come from the authoritative backend.
        decimal totalPrice = room.PricePerNight * stay.GetNumberOfNights();
        return Result<BookingPreparation>.Success(new BookingPreparation(room, stay, parkingSpaceId, totalPrice));
    }

    /// <summary>
    /// Converts DateRange construction exceptions into Result failures.
    /// DateRange throws because invalid ranges should not exist in the domain; the
    /// application layer translates that into the Result pattern used by use cases.
    /// </summary>
    private static Result<DateRange> CreateStay(DateTime checkIn, DateTime checkOut)
    {
        try
        {
            return Result<DateRange>.Success(new DateRange(checkIn, checkOut));
        }
        catch (ArgumentException exception)
        {
            return Result<DateRange>.Failure(new Error("Booking.InvalidStay", exception.Message));
        }
    }
}
