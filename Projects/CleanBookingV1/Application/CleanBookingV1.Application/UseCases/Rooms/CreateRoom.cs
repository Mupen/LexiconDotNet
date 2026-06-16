using CleanBookingV1.Application.Contracts;
using CleanBookingV1.Application.Interfaces;
using CleanBookingV1.Domain.Entities;

namespace CleanBookingV1.Application.UseCases.Rooms;

public sealed class CreateRoom
{
    private readonly IRoomRepository _roomRepository;
    private readonly IIntIdGenerator _idGenerator;

    public CreateRoom(
        IRoomRepository roomRepository,
        IIntIdGenerator idGenerator)
    {
        _roomRepository = roomRepository
            ?? throw new ArgumentNullException(nameof(roomRepository));

        _idGenerator = idGenerator
            ?? throw new ArgumentNullException(nameof(idGenerator));
    }

    public Result<Room> Execute(CreateRoomRequest request)
    {
        var validationResult = ValidateRequest(request);
        if (validationResult.IsFailure)
        {
            return Result<Room>.Failure(validationResult.Error);
        }

        int newId = _idGenerator.NextId();

        var roomResult = CreateRoomEntity(newId, request!);
        if (roomResult.IsFailure)
        {
            return roomResult;
        }

        _roomRepository.Add(roomResult.Value!);

        return roomResult;
    }

    private static Result ValidateRequest(CreateRoomRequest? request)
    {
        if (request is null)
        {
            return Result.Failure(
                new Error("Room.Create.NullRequest", "Request cannot be null."));
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result.Failure(
                new Error("Room.Create.InvalidName", "Room name is required."));
        }

        return Result.Success();
    }

    private static Result<Room> CreateRoomEntity(int id, CreateRoomRequest request)
    {
        try
        {
            var room = new Room(
                id: id,
                name: request.Name,
                roomType: request.RoomType,
                sizeInSquareMeters: request.SizeInSquareMeters,
                capacity: request.Capacity,
                pricePerNight: request.PricePerNight,
                isActive: request.IsActive);

            return Result<Room>.Success(room);
        }
        catch (ArgumentException ex)
        {
            return Result<Room>.Failure(
                new Error("Room.Create.InvalidRoom", ex.Message));
        }
    }
}