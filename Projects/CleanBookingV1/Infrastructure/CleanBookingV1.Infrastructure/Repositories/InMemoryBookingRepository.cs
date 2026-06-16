using CleanBookingV1.Application.Interfaces;
using CleanBookingV1.Domain.Entities;

namespace CleanBookingV1.Infrastructure.Repositories;

public sealed class InMemoryBookingRepository : IBookingRepository
{
    private readonly List<Booking> _bookings = [];

    public IReadOnlyList<Booking> GetAll()
    {
        return _bookings;
    }

    public Booking? GetById(Guid id)
    {
        return _bookings.FirstOrDefault(booking => booking.Id == id);
    }

    public IReadOnlyList<Booking> GetByRoomId(int roomId)
    {
        return _bookings
            .Where(booking => booking.RoomId == roomId)
            .ToList();
    }

    public IReadOnlyList<Booking> GetByParkingSpaceId(int parkingSpaceId)
    {
        return _bookings
            .Where(booking => booking.ParkingSpaceId == parkingSpaceId)
            .ToList();
    }

    public void Add(Booking booking)
    {
        if (booking is null)
        {
            throw new ArgumentNullException(nameof(booking));
        }

        _bookings.Add(booking);
    }

    public void Update(Booking booking)
    {
        if (booking is null)
        {
            throw new ArgumentNullException(nameof(booking));
        }

        var index = _bookings.FindIndex(existingBooking => existingBooking.Id == booking.Id);

        if (index == -1)
        {
            throw new InvalidOperationException($"Booking with id '{booking.Id}' was not found.");
        }

        _bookings[index] = booking;
    }

    public void Delete(Guid id)
    {
        var booking = GetById(id);

        if (booking is null)
        {
            throw new InvalidOperationException($"Booking with id '{id}' was not found.");
        }

        _bookings.Remove(booking);
    }
}