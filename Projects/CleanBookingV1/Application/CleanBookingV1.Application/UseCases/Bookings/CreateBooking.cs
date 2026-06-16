using CleanBookingV1.Application.Contracts;
using CleanBookingV1.Application.Interfaces;
using CleanBookingV1.Application.Requests.Bookings;
using CleanBookingV1.Domain.Entities;
using CleanBookingV1.Domain.ValueObjects;

namespace CleanBookingV1.Application.UseCases.Bookings;

public sealed class CreateBooking
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IParkingSpaceRepository _parkingSpaceRepository;
    private readonly IGuidGenerator _guidGenerator;

    public CreateBooking(
        IBookingRepository bookingRepository, 
        IRoomRepository roomRepository, 
        IParkingSpaceRepository parkingSpaceRepository, 
        IGuidGenerator guidGenerator) 
    {
        _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
        _roomRepository = roomRepository ?? throw new ArgumentNullException(nameof(roomRepository));
        _parkingSpaceRepository = parkingSpaceRepository ?? throw new ArgumentNullException(nameof(parkingSpaceRepository));
        _guidGenerator = guidGenerator ?? throw new ArgumentNullException(nameof(guidGenerator));
    }

    public Result<Booking> Execute(CreateBookingRequest request)
    {
        var requestValidation = ValidateRequest(request);
        if (requestValidation.IsFailure)
            return Result<Booking>.Failure(requestValidation.Error);

        CreateBookingRequest validatedRequest = request!;
        var roomResult = GetAvailableRoom(validatedRequest);

        if (roomResult.IsFailure)
            return Result<Booking>.Failure(roomResult.Error);

        var room = roomResult.Value!;

        var stayResult = CreateStay(request.CheckIn, request.CheckOut);
        if (stayResult.IsFailure)
            return Result<Booking>.Failure(stayResult.Error);

        var stay = stayResult.Value!;

        var roomAvailabilityResult = CheckRoomAvailability(room.Id, stay);
        if (roomAvailabilityResult.IsFailure)
            return Result<Booking>.Failure(roomAvailabilityResult.Error);

        var parkingResult = FindAvailableParkingSpaceId(request.ParkingRequested, stay);
        if (parkingResult.IsFailure)
            return Result<Booking>.Failure(parkingResult.Error);

        decimal totalPrice = CalculateTotalPrice(room.PricePerNight, stay);
        Guid bookingId = _guidGenerator.NewGuid();

        var bookingResult = CreateBookingEntity(
            bookingId,
            request,
            stay,
            room.Id,
            parkingResult.Value,
            totalPrice);

        if (bookingResult.IsFailure)
        {
            return bookingResult;
        }

        _bookingRepository.Add(bookingResult.Value!);

        return bookingResult;
    }


    private static Result ValidateRequest(CreateBookingRequest? request)
    {
        if (request is null)
        {
            return Result.Failure(
                new Error("Booking.Create.NullRequest", "Request cannot be null."));
        }

        return Result.Success();
    }

    private Result<Room> GetAvailableRoom(CreateBookingRequest request)
    {
        Room? room = _roomRepository.GetById(request.RoomId);

        if (room is null)
        {
            return Result<Room>.Failure(
                new Error("Booking.Create.RoomNotFound", "The selected room was not found."));
        }

        if (!room.IsActive)
        {
            return Result<Room>.Failure(
                new Error("Booking.Create.RoomInactive", "The selected room is inactive."));
        }

        if (request.NumberOfGuests > room.Capacity)
        {
            return Result<Room>.Failure(
                new Error(
                    "Booking.Create.RoomCapacityExceeded",
                    $"Room '{room.Name}' allows a maximum of {room.Capacity} guest(s)."));
        }

        return Result<Room>.Success(room);
    }

    private static Result<DateRange> CreateStay(DateTime checkIn, DateTime checkOut)
    {
        try
        {
            var stay = new DateRange(checkIn, checkOut);
            return Result<DateRange>.Success(stay);
        }
        catch (ArgumentException exception)
        {
            return Result<DateRange>.Failure(
                new Error("Booking.Create.InvalidStay", exception.Message));
        }
    }

    private Result CheckRoomAvailability(int roomId, DateRange stay)
    {
        IReadOnlyList<Booking> roomBookings = _bookingRepository.GetByRoomId(roomId);

        bool roomIsUnavailable = roomBookings.Any(existingBooking =>
            existingBooking.Overlaps(stay));

        if (roomIsUnavailable)
        {
            return Result.Failure(
                new Error(
                    "Booking.Create.RoomUnavailable",
                    "The selected room is not available for the requested stay."));
        }

        return Result.Success();
    }

    private Result<int?> FindAvailableParkingSpaceId(bool parkingRequested, DateRange stay)
    {
        if (!parkingRequested)
        {
            return Result<int?>.Success(null);
        }

        IReadOnlyList<ParkingSpace> activeParkingSpaces = _parkingSpaceRepository
            .GetAll()
            .Where(space => space.IsActive)
            .ToList();

        ParkingSpace? availableParkingSpace = activeParkingSpaces.FirstOrDefault(space =>
        {
            IReadOnlyList<Booking> parkingBookings =
                _bookingRepository.GetByParkingSpaceId(space.Id);

            return parkingBookings.All(existingBooking => !existingBooking.Overlaps(stay));
        });

        if (availableParkingSpace is null)
        {
            return Result<int?>.Failure(
                new Error(
                    "Booking.Create.ParkingUnavailable",
                    "No parking space is available for the requested stay."));
        }

        return Result<int?>.Success(availableParkingSpace.Id);
    }

    private static decimal CalculateTotalPrice(decimal pricePerNight, DateRange stay)
    {
        return pricePerNight * stay.GetNumberOfNights();
    }

    private static Result<Booking> CreateBookingEntity(
        Guid bookingId,
        CreateBookingRequest request,
        DateRange stay,
        int roomId,
        int? parkingSpaceId,
        decimal totalPrice)
    {
        try
        {
            var booking = new Booking(
                id: bookingId,
                guestName: request.GuestName,
                stay: stay,
                numberOfGuests: request.NumberOfGuests,
                roomId: roomId,
                parkingSpaceId: parkingSpaceId,
                totalPrice: totalPrice,
                estimatedArrivalTime: request.EstimatedArrivalTime);

            return Result<Booking>.Success(booking);
        }
        catch (ArgumentException exception)
        {
            return Result<Booking>.Failure(
                new Error("Booking.Create.InvalidBooking", exception.Message));
        }
    }
}