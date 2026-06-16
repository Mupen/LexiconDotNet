using CleanBookingV1.Application.Contracts;
using CleanBookingV1.Application.Interfaces;
using CleanBookingV1.Application.Requests.Rooms;

namespace CleanBookingV1.Application.UseCases.Rooms;

public sealed class ActivateRoom
{
    private readonly IRoomRepository _roomRepository;

    public ActivateRoom(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository
            ?? throw new ArgumentNullException(nameof(roomRepository));
    }

    public Result Execute(ActivateRoomRequest request)
    {
        if (request is null)
        {
            return Result.Failure(
                new Error("Room.Activate.NullRequest", "Request cannot be null."));
        }

        if (request.RoomId <= 0)
        {
            return Result.Failure(
                new Error("Room.Activate.InvalidId", "Room id must be greater than zero."));
        }

        var room = _roomRepository.GetById(request.RoomId);

        if (room is null)
        {
            return Result.Failure(
                new Error("Room.Activate.NotFound", "The room was not found."));
        }

        room.Activate();

        _roomRepository.Update(room);

        return Result.Success();
    }
}