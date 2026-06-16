using CleanBookingV2.Application.Interfaces;
using CleanBookingV2.Application.Requests.Bookings;
using CleanBookingV2.Application.Services;
using CleanBookingV2.Application.UseCases.Bookings;
using CleanBookingV2.Domain.Contracts;
using CleanBookingV2.Domain.Entities;
using CleanBookingV2.Domain.Enums;
using CleanBookingV2.Domain.ValueObjects;

namespace CleanBookingV2.UnitTests.Application;

/// <summary>
/// Tests booking use cases with in-memory test doubles.
/// This keeps the tests focused on application behavior without needing SQLite or
/// ASP.NET Core. The fake repositories model the contracts that infrastructure
/// implements in production.
/// </summary>
public sealed class BookingUseCaseTests
{
    [Fact]
    public async Task Create_ReturnsFailure_WhenRoomOverlapsActiveBooking()
    {
        // A room can only have one active booking for an overlapping stay.
        var context = TestContext.Create();
        context.Bookings.Add(CreateExistingBooking(roomId: 1));

        var useCase = context.CreateBookingUseCase();
        var result = await useCase.ExecuteAsync(CreateRequest(roomId: 1), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Booking.RoomUnavailable", result.Error?.Code);
    }

    [Fact]
    public async Task Create_ReturnsFailure_WhenSelectedParkingOverlapsActiveBooking()
    {
        // Parking spaces follow the same overlap rule as rooms.
        var context = TestContext.Create();
        context.Bookings.Add(CreateExistingBooking(roomId: 2, parkingSpaceId: 1));

        var useCase = context.CreateBookingUseCase();
        var result = await useCase.ExecuteAsync(CreateRequest(roomId: 1, parkingSpaceId: 1), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Booking.ParkingUnavailable", result.Error?.Code);
    }

    [Fact]
    public async Task Create_Succeeds_WhenOverlappingBookingIsCancelled()
    {
        // Cancelled bookings are kept for history, but they must not block new bookings.
        var context = TestContext.Create();
        var cancelledBooking = CreateExistingBooking(roomId: 1);
        cancelledBooking.Cancel();
        context.Bookings.Add(cancelledBooking);

        var useCase = context.CreateBookingUseCase();
        var result = await useCase.ExecuteAsync(CreateRequest(roomId: 1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(context.Bookings, booking => booking.IsActive());
    }

    [Fact]
    public async Task Create_ReturnsFailure_WhenRoomDoesNotExist()
    {
        // The API must reject bookings for unknown rooms instead of creating orphaned data.
        var context = TestContext.Create();

        var useCase = context.CreateBookingUseCase();
        var result = await useCase.ExecuteAsync(CreateRequest(roomId: 999), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Booking.RoomNotFound", result.Error?.Code);
    }

    [Fact]
    public async Task Create_ReturnsFailure_WhenGuestCountExceedsRoomCapacity()
    {
        // Room capacity is authoritative backend data and cannot be bypassed by the frontend.
        var context = TestContext.Create();

        var useCase = context.CreateBookingUseCase();
        var result = await useCase.ExecuteAsync(CreateRequest(roomId: 1, numberOfGuests: 3), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Booking.RoomCapacityExceeded", result.Error?.Code);
    }

    [Fact]
    public async Task Create_Succeeds_WhenOverlappingParkingBookingIsCancelled()
    {
        // A cancelled parking booking should release the parking space just like it releases a room.
        var context = TestContext.Create();
        var cancelledBooking = CreateExistingBooking(roomId: 2, parkingSpaceId: 1);
        cancelledBooking.Cancel();
        context.Bookings.Add(cancelledBooking);

        var useCase = context.CreateBookingUseCase();
        var result = await useCase.ExecuteAsync(CreateRequest(roomId: 1, parkingSpaceId: 1), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Update_Succeeds_WhenBookingKeepsItsOwnRoomAndParking()
    {
        // Updating a booking should ignore its own existing room and parking reservations.
        var context = TestContext.Create();
        var booking = CreateExistingBooking(roomId: 1, parkingSpaceId: 1);
        context.Bookings.Add(booking);

        var useCase = context.UpdateBookingUseCase();
        var result = await useCase.ExecuteAsync(
            new UpdateBookingRequest(
                booking.Id,
                "Updated Guest",
                new DateTime(2026, 6, 12, 14, 0, 0),
                new DateTime(2026, 6, 14, 12, 0, 0),
                2,
                1,
                1,
                null),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Updated Guest", booking.GuestName);
        Assert.Equal(1, booking.ParkingSpaceId);
    }

    [Fact]
    public async Task Update_ReturnsFailure_WhenMovingToUnavailableRoom()
    {
        // Moving an existing booking must re-check availability against other active bookings.
        var context = TestContext.Create();
        var booking = CreateExistingBooking(roomId: 1);
        context.Bookings.Add(booking);
        context.Bookings.Add(CreateExistingBooking(roomId: 2));

        var useCase = context.UpdateBookingUseCase();
        var result = await useCase.ExecuteAsync(
            new UpdateBookingRequest(
                booking.Id,
                "Updated Guest",
                new DateTime(2026, 6, 12, 14, 0, 0),
                new DateTime(2026, 6, 14, 12, 0, 0),
                2,
                2,
                null,
                null),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Booking.RoomUnavailable", result.Error?.Code);
    }

    [Fact]
    public async Task Cancel_MarksBookingCancelled()
    {
        // Cancel is a soft delete: it keeps the booking record but removes it from active availability.
        var context = TestContext.Create();
        var booking = CreateExistingBooking(roomId: 1, parkingSpaceId: 1);
        context.Bookings.Add(booking);

        var useCase = context.CancelBookingUseCase();
        var result = await useCase.ExecuteAsync(booking.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(booking.IsActive());
    }

    private static CreateBookingRequest CreateRequest(int roomId, int? parkingSpaceId = null, int numberOfGuests = 2)
    {
        return new CreateBookingRequest(
            "New Guest",
            new DateTime(2026, 6, 12, 14, 0, 0),
            new DateTime(2026, 6, 14, 12, 0, 0),
            numberOfGuests,
            roomId,
            parkingSpaceId,
            null);
    }

    /// <summary>
    /// Creates an existing active booking used by overlap tests.
    /// Centralizing the setup keeps all tests using the same stay window so each
    /// test can focus on the one rule it is proving.
    /// </summary>
    private static Booking CreateExistingBooking(int roomId, int? parkingSpaceId = null)
    {
        return new Booking(
            Guid.NewGuid(),
            "Existing Guest",
            new DateRange(
                new DateTime(2026, 6, 12, 14, 0, 0),
                new DateTime(2026, 6, 14, 12, 0, 0)),
            2,
            roomId,
            parkingSpaceId,
            1400m,
            null);
    }

    /// <summary>
    /// Small in-memory composition root for application tests.
    /// It mirrors production dependency injection with fake repositories so the use
    /// cases are tested through their real public API.
    /// </summary>
    private sealed class TestContext
    {
        private readonly InMemoryBookingRepository _bookingRepository;
        private readonly InMemoryRoomRepository _roomRepository;
        private readonly InMemoryParkingSpaceRepository _parkingSpaceRepository;
        private readonly FixedGuidGenerator _guidGenerator = new();
        private readonly PassthroughBookingTransaction _transaction = new();

        private TestContext()
        {
            _bookingRepository = new InMemoryBookingRepository();
            _roomRepository = new InMemoryRoomRepository(
                new Room(1, "Room 1", RoomType.Double, 14, 2, 700m),
                new Room(2, "Room 2", RoomType.Double, 16, 2, 765m));
            _parkingSpaceRepository = new InMemoryParkingSpaceRepository(
                new ParkingSpace(1, "Parking Space 1", ParkingSpaceType.Standard),
                new ParkingSpace(2, "Parking Space 2", ParkingSpaceType.Standard));
        }

        public List<Booking> Bookings => _bookingRepository.Bookings;

        public static TestContext Create()
        {
            return new TestContext();
        }

        public CreateBooking CreateBookingUseCase()
        {
            return new CreateBooking(
                _bookingRepository,
                _guidGenerator,
                CreatePreparationService(),
                _transaction);
        }

        public UpdateBooking UpdateBookingUseCase()
        {
            return new UpdateBooking(
                _bookingRepository,
                CreatePreparationService(),
                _transaction);
        }

        public CancelBooking CancelBookingUseCase()
        {
            return new CancelBooking(_bookingRepository);
        }

        private BookingPreparationService CreatePreparationService()
        {
            return new BookingPreparationService(
                _roomRepository,
                new BookingAvailabilityService(_bookingRepository, _parkingSpaceRepository));
        }
    }

    private sealed class InMemoryBookingRepository : IBookingRepository
    {
        public List<Booking> Bookings { get; } = [];

        public Task AddAsync(Booking booking, CancellationToken cancellationToken)
        {
            Bookings.Add(booking);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<Booking>> GetAllAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<Booking>>(Bookings);
        }

        public Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Bookings.FirstOrDefault(booking => booking.Id == id));
        }

        public Task<IReadOnlyList<Booking>> GetByParkingSpaceIdAsync(int parkingSpaceId, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<Booking>>(Bookings.Where(booking => booking.ParkingSpaceId == parkingSpaceId).ToList());
        }

        public Task<IReadOnlyList<Booking>> GetByRoomIdAsync(int roomId, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<Booking>>(Bookings.Where(booking => booking.RoomId == roomId).ToList());
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class InMemoryRoomRepository : IRoomRepository
    {
        private readonly IReadOnlyList<Room> _rooms;

        public InMemoryRoomRepository(params Room[] rooms)
        {
            _rooms = rooms;
        }

        public Task<IReadOnlyList<Room>> GetAllAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_rooms);
        }

        public Task<Room?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_rooms.FirstOrDefault(room => room.Id == id));
        }
    }

    private sealed class InMemoryParkingSpaceRepository : IParkingSpaceRepository
    {
        private readonly IReadOnlyList<ParkingSpace> _parkingSpaces;

        public InMemoryParkingSpaceRepository(params ParkingSpace[] parkingSpaces)
        {
            _parkingSpaces = parkingSpaces;
        }

        public Task<IReadOnlyList<ParkingSpace>> GetAllAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_parkingSpaces);
        }

        public Task<ParkingSpace?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_parkingSpaces.FirstOrDefault(space => space.Id == id));
        }
    }

    private sealed class FixedGuidGenerator : IGuidGenerator
    {
        public Guid NewGuid()
        {
            return Guid.Parse("1f4df784-f418-4a20-98e1-ea144e4da001");
        }
    }

    private sealed class PassthroughBookingTransaction : IBookingTransaction
    {
        public Task<Result<T>> ExecuteAsync<T>(
            Func<CancellationToken, Task<Result<T>>> operation,
            CancellationToken cancellationToken)
        {
            return operation(cancellationToken);
        }

        public Task<Result> ExecuteAsync(
            Func<CancellationToken, Task<Result>> operation,
            CancellationToken cancellationToken)
        {
            return operation(cancellationToken);
        }
    }
}
