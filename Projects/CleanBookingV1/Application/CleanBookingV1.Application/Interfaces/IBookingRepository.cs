using CleanBookingV1.Domain.Entities;

namespace CleanBookingV1.Application.Interfaces;

public interface IBookingRepository
{
    IReadOnlyList<Booking> GetAll();
    Booking? GetById(Guid id);

    IReadOnlyList<Booking> GetByRoomId(int roomId);
    IReadOnlyList<Booking> GetByParkingSpaceId(int parkingSpaceId);

    void Add(Booking booking);
    void Update(Booking booking);
    void Delete(Guid id);
}