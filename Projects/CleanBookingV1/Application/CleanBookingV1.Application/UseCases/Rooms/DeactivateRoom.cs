using CleanBookingV1.Application.Contracts;
using CleanBookingV1.Application.Interfaces;
using CleanBookingV1.Application.Requests.Rooms;

namespace CleanBookingV1.Application.UseCases.Rooms;

public sealed class DeactivateRoom
{
    private readonly IRoomRepository _roomRepository;

    public DeactivateRoom(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository
            ?? throw new ArgumentNullException(nameof(roomRepository));
    }

    public Result Execute(DeactivateRoomRequest request)
    {
        if (request is null)
        {
            return Result.Failure(
                new Error("Room.Deactivate.NullRequest", "Request cannot be null."));
        }

        if (request.RoomId <= 0)
        {
            return Result.Failure(
                new Error("Room.Deactivate.InvalidId", "Room id must be greater than zero."));
        }

        var room = _roomRepository.GetById(request.RoomId);

        if (room is null)
        {
            return Result.Failure(
                new Error("Room.Deactivate.NotFound", "The room was not found."));
        }

        room.Deactivate();

        _roomRepository.Update(room);

        return Result.Success();
    }
}