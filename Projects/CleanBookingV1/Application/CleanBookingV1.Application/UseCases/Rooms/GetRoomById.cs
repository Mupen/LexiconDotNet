using CleanBookingV1.Application.Contracts;
using CleanBookingV1.Application.Interfaces;
using CleanBookingV1.Application.Requests.Rooms;
using CleanBookingV1.Domain.Entities;

namespace CleanBookingV1.Application.UseCases.Rooms;

public sealed class GetRoomById
{
    private readonly IRoomRepository _roomRepository;

    public GetRoomById(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository
            ?? throw new ArgumentNullException(nameof(roomRepository));
    }

    public Result<Room> Execute(GetRoomByIdRequest request)
    {
        if (request is null)
        {
            return Result<Room>.Failure(
                new Error("Room.GetById.NullRequest", "Request cannot be null."));
        }

        if (request.RoomId <= 0)
        {
            return Result<Room>.Failure(
                new Error("Room.GetById.InvalidId", "Room id must be greater than zero."));
        }

        var room = _roomRepository.GetById(request.RoomId);

        if (room is null)
        {
            return Result<Room>.Failure(
                new Error("Room.GetById.NotFound", "The room was not found."));
        }

        return Result<Room>.Success(room);
    }
}