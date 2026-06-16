using CleanBookingV1.Application.Contracts;
using CleanBookingV1.Application.Interfaces;
using CleanBookingV1.Domain.Entities;

namespace CleanBookingV1.Application.UseCases.Rooms;

public sealed class GetAllRooms
{
    private readonly IRoomRepository _roomRepository;

    public GetAllRooms(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository
            ?? throw new ArgumentNullException(nameof(roomRepository));
    }

    public Result<IReadOnlyList<Room>> Execute()
    {
        var rooms = _roomRepository.GetAll();

        return Result<IReadOnlyList<Room>>.Success(rooms);
    }
}