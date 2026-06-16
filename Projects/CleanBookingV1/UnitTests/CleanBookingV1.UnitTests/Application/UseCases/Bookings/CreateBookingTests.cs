using CleanBookingV1.Application.Requests.Bookings;
using CleanBookingV1.Domain.Entities;
using CleanBookingV1.Domain.ValueObjects;
using CleanBookingV1.Infrastructure.Repositories;
using CleanBookingV1.UnitTests.Helpers.TestDoubles.Stubs;
using FluentAssertions;

namespace CleanBookingV1.UnitTests.Application.UseCases.Bookings;

public sealed class CreateBookingTests
{
    [Fact]
    public void Execute_Should_CreateBooking_When_RequestIsValid()
    {
        var bookingRepository = new InMemoryBookingRepository();
        var roomRepository = new InMemoryRoomRepository();
        var parkingSpaceRepository = new InMemoryParkingSpaceRepository();
        var fixedGuid = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var guidGenerator = new FixedGuidGenerator(fixedGuid);

        var useCase = new CreateBooking(
            bookingRepository,
            roomRepository,
            parkingSpaceRepository,
            guidGenerator);

        var request = new CreateBookingRequest(
            GuestName: "Daniel",
            RoomId: 1,
            CheckIn: new DateTime(2026, 4, 10),
            CheckOut: new DateTime(2026, 4, 12),
            NumberOfGuests: 1,
            ParkingRequested: false,
            EstimatedArrivalTime: null);

        var result = useCase.Execute(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(fixedGuid);
        result.Value.GuestName.Should().Be("Daniel");
        result.Value.RoomId.Should().Be(1);
        result.Value.ParkingSpaceId.Should().BeNull();
        result.Value.TotalPrice.Should().Be(1100m);

        bookingRepository.GetAll().Should().HaveCount(1);
    }

    [Fact]
    public void Execute_Should_CreateBooking_WithParking_When_ParkingIsRequested_AndAvailable()
    {
        var bookingRepository = new InMemoryBookingRepository();
        var roomRepository = new InMemoryRoomRepository();
        var parkingSpaceRepository = new InMemoryParkingSpaceRepository();
        var fixedGuid = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var guidGenerator = new FixedGuidGenerator(fixedGuid);

        var useCase = new CreateBooking(
            bookingRepository,
            roomRepository,
            parkingSpaceRepository,
            guidGenerator);

        var request = new CreateBookingRequest(
            GuestName: "Anna",
            RoomId: 1,
            CheckIn: new DateTime(2026, 4, 10),
            CheckOut: new DateTime(2026, 4, 12),
            NumberOfGuests: 1,
            ParkingRequested: true,
            EstimatedArrivalTime: null);

        var result = useCase.Execute(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.ParkingSpaceId.Should().NotBeNull();
        bookingRepository.GetAll().Should().HaveCount(1);
    }

    [Fact]
    public void Execute_Should_Fail_When_RequestIsNull()
    {
        var bookingRepository = new InMemoryBookingRepository();
        var roomRepository = new InMemoryRoomRepository();
        var parkingSpaceRepository = new InMemoryParkingSpaceRepository();
        var guidGenerator = new FixedGuidGenerator(Guid.NewGuid());

        var useCase = new CreateBooking(
            bookingRepository,
            roomRepository,
            parkingSpaceRepository,
            guidGenerator);

        CreateBookingRequest? request = null;

        var result = useCase.Execute(request!);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Booking.Create.NullRequest");
    }

    [Fact]
    public void Execute_Should_Fail_When_RoomDoesNotExist()
    {
        var bookingRepository = new InMemoryBookingRepository();
        var roomRepository = new InMemoryRoomRepository();
        var parkingSpaceRepository = new InMemoryParkingSpaceRepository();
        var guidGenerator = new FixedGuidGenerator(Guid.NewGuid());

        var useCase = new CreateBooking(
            bookingRepository,
            roomRepository,
            parkingSpaceRepository,
            guidGenerator);

        var request = new CreateBookingRequest(
            GuestName: "Daniel",
            RoomId: 999,
            CheckIn: new DateTime(2026, 4, 10),
            CheckOut: new DateTime(2026, 4, 12),
            NumberOfGuests: 1,
            ParkingRequested: false,
            EstimatedArrivalTime: null);

        var result = useCase.Execute(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Booking.Create.RoomNotFound");
    }

    [Fact]
    public void Execute_Should_Fail_When_RoomIsInactive()
    {
        var bookingRepository = new InMemoryBookingRepository();
        var roomRepository = new InMemoryRoomRepository();
        var parkingSpaceRepository = new InMemoryParkingSpaceRepository();
        var guidGenerator = new FixedGuidGenerator(Guid.NewGuid());

        var room = roomRepository.GetById(1)!;
        room.Deactivate();
        roomRepository.Update(room);

        var useCase = new CreateBooking(
            bookingRepository,
            roomRepository,
            parkingSpaceRepository,
            guidGenerator);

        var request = new CreateBookingRequest(
            GuestName: "Daniel",
            RoomId: 1,
            CheckIn: new DateTime(2026, 4, 10),
            CheckOut: new DateTime(2026, 4, 12),
            NumberOfGuests: 1,
            ParkingRequested: false,
            EstimatedArrivalTime: null);

        var result = useCase.Execute(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Booking.Create.RoomInactive");
    }

    [Fact]
    public void Execute_Should_Fail_When_NumberOfGuests_ExceedsRoomCapacity()
    {
        var bookingRepository = new InMemoryBookingRepository();
        var roomRepository = new InMemoryRoomRepository();
        var parkingSpaceRepository = new InMemoryParkingSpaceRepository();
        var guidGenerator = new FixedGuidGenerator(Guid.NewGuid());

        var useCase = new CreateBooking(
            bookingRepository,
            roomRepository,
            parkingSpaceRepository,
            guidGenerator);

        var request = new CreateBookingRequest(
            GuestName: "Daniel",
            RoomId: 1,
            CheckIn: new DateTime(2026, 4, 10),
            CheckOut: new DateTime(2026, 4, 12),
            NumberOfGuests: 2,
            ParkingRequested: false,
            EstimatedArrivalTime: null);

        var result = useCase.Execute(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Booking.Create.RoomCapacityExceeded");
    }

    [Fact]
    public void Execute_Should_Fail_When_StayIsInvalid()
    {
        var bookingRepository = new InMemoryBookingRepository();
        var roomRepository = new InMemoryRoomRepository();
        var parkingSpaceRepository = new InMemoryParkingSpaceRepository();
        var guidGenerator = new FixedGuidGenerator(Guid.NewGuid());

        var useCase = new CreateBooking(
            bookingRepository,
            roomRepository,
            parkingSpaceRepository,
            guidGenerator);

        var request = new CreateBookingRequest(
            GuestName: "Daniel",
            RoomId: 1,
            CheckIn: new DateTime(2026, 4, 12),
            CheckOut: new DateTime(2026, 4, 10),
            NumberOfGuests: 1,
            ParkingRequested: false,
            EstimatedArrivalTime: null);

        var result = useCase.Execute(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Booking.Create.InvalidStay");
    }

    [Fact]
    public void Execute_Should_Fail_When_RoomIsAlreadyBooked_ForSamePeriod()
    {
        var bookingRepository = new InMemoryBookingRepository();
        var roomRepository = new InMemoryRoomRepository();
        var parkingSpaceRepository = new InMemoryParkingSpaceRepository();
        var guidGenerator = new FixedGuidGenerator(Guid.NewGuid());

        bookingRepository.Add(CreateExistingBooking(
            id: Guid.NewGuid(),
            guestName: "Existing Guest",
            roomId: 1,
            parkingSpaceId: null,
            checkIn: new DateTime(2026, 4, 10),
            checkOut: new DateTime(2026, 4, 12),
            totalPrice: 1100m));

        var useCase = new CreateBooking(
            bookingRepository,
            roomRepository,
            parkingSpaceRepository,
            guidGenerator);

        var request = new CreateBookingRequest(
            GuestName: "Daniel",
            RoomId: 1,
            CheckIn: new DateTime(2026, 4, 11),
            CheckOut: new DateTime(2026, 4, 13),
            NumberOfGuests: 1,
            ParkingRequested: false,
            EstimatedArrivalTime: null);

        var result = useCase.Execute(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Booking.Create.RoomUnavailable");
    }

    [Fact]
    public void Execute_Should_Fail_When_NoParkingSpaceIsAvailable()
    {
        var bookingRepository = new InMemoryBookingRepository();
        var roomRepository = new InMemoryRoomRepository();
        var parkingSpaceRepository = new InMemoryParkingSpaceRepository();
        var guidGenerator = new FixedGuidGenerator(Guid.NewGuid());

        bookingRepository.Add(CreateExistingBooking(
            id: Guid.NewGuid(),
            guestName: "Guest 1",
            roomId: 1,
            parkingSpaceId: 1,
            checkIn: new DateTime(2026, 4, 10),
            checkOut: new DateTime(2026, 4, 12),
            totalPrice: 1100m));

        bookingRepository.Add(CreateExistingBooking(
            id: Guid.NewGuid(),
            guestName: "Guest 2",
            roomId: 2,
            parkingSpaceId: 2,
            checkIn: new DateTime(2026, 4, 10),
            checkOut: new DateTime(2026, 4, 12),
            totalPrice: 1400m));

        var useCase = new CreateBooking(
            bookingRepository,
            roomRepository,
            parkingSpaceRepository,
            guidGenerator);

        var request = new CreateBookingRequest(
            GuestName: "Daniel",
            RoomId: 3,
            CheckIn: new DateTime(2026, 4, 10),
            CheckOut: new DateTime(2026, 4, 12),
            NumberOfGuests: 2,
            ParkingRequested: true,
            EstimatedArrivalTime: null);

        var result = useCase.Execute(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Booking.Create.ParkingUnavailable");
    }

    [Fact]
    public void Execute_Should_Fail_When_BookingEntityIsInvalid()
    {
        var bookingRepository = new InMemoryBookingRepository();
        var roomRepository = new InMemoryRoomRepository();
        var parkingSpaceRepository = new InMemoryParkingSpaceRepository();
        var guidGenerator = new FixedGuidGenerator(Guid.NewGuid());

        var useCase = new CreateBooking(
            bookingRepository,
            roomRepository,
            parkingSpaceRepository,
            guidGenerator);

        var request = new CreateBookingRequest(
            GuestName: "",
            RoomId: 1,
            CheckIn: new DateTime(2026, 4, 10),
            CheckOut: new DateTime(2026, 4, 12),
            NumberOfGuests: 1,
            ParkingRequested: false,
            EstimatedArrivalTime: null);

        var result = useCase.Execute(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Booking.Create.InvalidBooking");
    }

    private static Booking CreateExistingBooking(
        Guid id,
        string guestName,
        int roomId,
        int? parkingSpaceId,
        DateTime checkIn,
        DateTime checkOut,
        decimal totalPrice)
    {
        return new Booking(
            id: id,
            guestName: guestName,
            stay: new DateRange(checkIn, checkOut),
            numberOfGuests: 1,
            roomId: roomId,
            parkingSpaceId: parkingSpaceId,
            totalPrice: totalPrice,
            estimatedArrivalTime: null);
    }
}