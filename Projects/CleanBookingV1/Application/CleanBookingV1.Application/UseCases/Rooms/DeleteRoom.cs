using CleanBookingV1.Application.Contracts;
using CleanBookingV1.Application.Interfaces;
using CleanBookingV1.Application.Requests.Rooms;

namespace CleanBookingV1.Application.UseCases.Rooms;

public sealed class DeleteRoom
{
    private readonly IRoomRepository _roomRepository;

    public DeleteRoom(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository
            ?? throw new ArgumentNullException(nameof(roomRepository));
    }

    public Result Execute(DeleteRoomRequest request)
    {
        if (request is null)
        {
            return Result.Failure(
                new Error("Room.Delete.NullRequest", "Request cannot be null."));
        }

        if (request.RoomId <= 0)
        {
            return Result.Failure(
                new Error("Room.Delete.InvalidId", "Room id must be greater than zero."));
        }

        var room = _roomRepository.GetById(request.RoomId);

        if (room is null)
        {
            return Result.Failure(
                new Error("Room.Delete.NotFound", "The room was not found."));
        }

        _roomRepository.Delete(request.RoomId);

        return Result.Success();
    }
}