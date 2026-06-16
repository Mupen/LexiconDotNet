using CleanBookingV1.Application.Contracts;
using CleanBookingV1.Application.Interfaces;
using CleanBookingV1.Domain.Entities;

namespace CleanBookingV1.Application.UseCases.Rooms;

public sealed class UpdateRoom
{
    private readonly IRoomRepository _roomRepository;

    public UpdateRoom(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository
            ?? throw new ArgumentNullException(nameof(roomRepository));
    }

    public Result<Room> Execute(UpdateRoomRequest request)
    {
        if (request is null)
        {
            return Result<Room>.Failure(
                new Error("Room.Update.NullRequest", "Request cannot be null."));
        }

        if (request.RoomId <= 0)
        {
            return Result<Room>.Failure(
                new Error("Room.Update.InvalidId", "Room id must be greater than zero."));
        }

        var room = _roomRepository.GetById(request.RoomId);

        if (room is null)
        {
            return Result<Room>.Failure(
                new Error("Room.Update.NotFound", "The room was not found."));
        }

        try
        {
            room.UpdateDetails(
                name: request.Name,
                roomType: request.RoomType,
                sizeInSquareMeters: request.SizeInSquareMeters,
                capacity: request.Capacity,
                pricePerNight: request.PricePerNight);
        }
        catch (ArgumentException ex)
        {
            return Result<Room>.Failure(
                new Error("Room.Update.InvalidRoom", ex.Message));
        }

        _roomRepository.Update(room);

        return Result<Room>.Success(room);
    }
}