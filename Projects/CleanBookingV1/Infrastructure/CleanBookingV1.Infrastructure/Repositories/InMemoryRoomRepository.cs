using CleanBookingV1.Application.Interfaces;
using CleanBookingV1.Domain.Entities;
using CleanBookingV1.Domain.Enums;

namespace CleanBookingV1.Infrastructure.Repositories;

public sealed class InMemoryRoomRepository : IRoomRepository
{
    private readonly List<Room> _rooms =
    [
        new Room(
            id: 1,
            name: "Room 1",
            roomType: RoomType.Single,
            sizeInSquareMeters: 11,
            capacity: 1,
            pricePerNight: 550m),

        new Room(
            id: 2,
            name: "Room 2",
            roomType: RoomType.DoubleTwin,
            sizeInSquareMeters: 14,
            capacity: 2,
            pricePerNight: 700m),

        new Room(
            id: 3,
            name: "Room 3",
            roomType: RoomType.DoubleBed,
            sizeInSquareMeters: 16,
            capacity: 2,
            pricePerNight: 765m),

        new Room(
            id: 4,
            name: "Room 4",
            roomType: RoomType.Family,
            sizeInSquareMeters: 24,
            capacity: 3,
            pricePerNight: 850m)
    ];

    public IReadOnlyList<Room> GetAll()
    {
        return _rooms;
    }

    public Room? GetById(int id)
    {
        return _rooms.FirstOrDefault(room => room.Id == id);
    }

    public void Add(Room room)
    {
        if (room is null)
        {
            throw new ArgumentNullException(nameof(room));
        }

        var existingRoom = GetById(room.Id);

        if (existingRoom is not null)
        {
            throw new InvalidOperationException($"Room with id '{room.Id}' already exists.");
        }

        _rooms.Add(room);
    }

    public void Update(Room room)
    {
        if (room is null)
        {
            throw new ArgumentNullException(nameof(room));
        }

        var index = _rooms.FindIndex(existingRoom => existingRoom.Id == room.Id);

        if (index == -1)
        {
            throw new InvalidOperationException($"Room with id '{room.Id}' was not found.");
        }

        _rooms[index] = room;
    }

    public void Delete(int id)
    {
        var room = GetById(id);

        if (room is null)
        {
            throw new InvalidOperationException($"Room with id '{id}' was not found.");
        }

        _rooms.Remove(room);
    }
}