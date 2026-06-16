using CleanBookingV1.Application.Contracts;
using CleanBookingV1.Application.Interfaces;
using CleanBookingV1.Domain.Entities;
using CleanBookingV1.Domain.ValueObjects;

namespace CleanBookingV1.Application.UseCases.Bookings;

public sealed class UpdateBooking
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IParkingSpaceRepository _parkingSpaceRepository;

    public UpdateBooking(
        IBookingRepository bookingRepository,
        IRoomRepository roomRepository,
        IParkingSpaceRepository parkingSpaceRepository)
    {
        _bookingRepository = bookingRepository
            ?? throw new ArgumentNullException(nameof(bookingRepository));

        _roomRepository = roomRepository
            ?? throw new ArgumentNullException(nameof(roomRepository));

        _parkingSpaceRepository = parkingSpaceRepository
            ?? throw new ArgumentNullException(nameof(parkingSpaceRepository));
    }

    public Result<Booking> Execute(UpdateBookingRequest request)
    {
        if (request is null)
        {
            return Result<Booking>.Failure(
                new Error("Booking.Update.NullRequest", "Request cannot be null."));
        }

        if (request.BookingId == Guid.Empty)
        {
            return Result<Booking>.Failure(
                new Error("Booking.Update.InvalidId", "Booking id cannot be empty."));
        }

        var existingBooking = _bookingRepository.GetById(request.BookingId);

        if (existingBooking is null)
        {
            return Result<Booking>.Failure(
                new Error("Booking.Update.NotFound", "The booking was not found."));
        }

        var room = _roomRepository.GetById(request.RoomId);

        if (room is null)
        {
            return Result<Booking>.Failure(
                new Error("Booking.Update.RoomNotFound", "The selected room was not found."));
        }

        if (!room.IsActive)
        {
            return Result<Booking>.Failure(
                new Error("Booking.Update.RoomInactive", "The selected room is inactive."));
        }

        if (request.NumberOfGuests > room.Capacity)
        {
            return Result<Booking>.Failure(
                new Error(
                    "Booking.Update.RoomCapacityExceeded",
                    $"Room '{room.Name}' allows a maximum of {room.Capacity} guest(s)."));
        }

        DateRange stay;

        try
        {
            stay = new DateRange(request.CheckIn, request.CheckOut);
        }
        catch (ArgumentException ex)
        {
            return Result<Booking>.Failure(
                new Error("Booking.Update.InvalidStay", ex.Message));
        }

        // IMPORTANT: exclude current booking from overlap check
        var roomBookings = _bookingRepository.GetByRoomId(room.Id);

        bool roomUnavailable = roomBookings.Any(b =>
            b.Id != existingBooking.Id &&
            b.Overlaps(stay));

        if (roomUnavailable)
        {
            return Result<Booking>.Failure(
                new Error(
                    "Booking.Update.RoomUnavailable",
                    "The selected room is not available for the requested stay."));
        }

        int? parkingSpaceId = null;

        if (request.ParkingRequested)
        {
            var activeSpaces = _parkingSpaceRepository
                .GetAll()
                .Where(s => s.IsActive)
                .ToList();

            var availableSpace = activeSpaces.FirstOrDefault(space =>
            {
                var bookings = _bookingRepository.GetByParkingSpaceId(space.Id);

                return bookings.All(b =>
                    b.Id != existingBooking.Id &&
                    !b.Overlaps(stay));
            });

            if (availableSpace is null)
            {
                return Result<Booking>.Failure(
                    new Error(
                        "Booking.Update.ParkingUnavailable",
                        "No parking space is available for the requested stay."));
            }

            parkingSpaceId = availableSpace.Id;
        }

        decimal totalPrice = room.PricePerNight * stay.GetNumberOfNights();

        try
        {
            existingBooking.UpdateDetails(
                guestName: request.GuestName,
                stay: stay,
                numberOfGuests: request.NumberOfGuests,
                roomId: room.Id,
                parkingSpaceId: parkingSpaceId,
                totalPrice: totalPrice,
                estimatedArrivalTime: request.EstimatedArrivalTime);
        }
        catch (ArgumentException ex)
        {
            return Result<Booking>.Failure(
                new Error("Booking.Update.InvalidBooking", ex.Message));
        }

        _bookingRepository.Update(existingBooking);

        return Result<Booking>.Success(existingBooking);
    }
}