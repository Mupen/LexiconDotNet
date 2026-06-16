using CleanBookingV1.Domain.Entities;

namespace CleanBookingV1.Application.Interfaces;

public interface IRoomRepository
{
    IReadOnlyList<Room> GetAll();
    Room? GetById(int id);
    void Add(Room room);
    void Update(Room room);
    void Delete(int id);
}