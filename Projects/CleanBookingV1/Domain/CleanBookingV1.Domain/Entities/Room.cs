using CleanBookingV1.Domain.Enums;

namespace CleanBookingV1.Domain.Entities;

public sealed class Room
{
    public int Id { get; }
    public string Name { get; private set; }
    public RoomType RoomType { get; private set; }
    public int SizeInSquareMeters { get; private set; }
    public int Capacity { get; private set; }
    public decimal PricePerNight { get; private set; }
    public bool IsActive { get; private set; }

    public Room(
        int id,
        string name,
        RoomType roomType,
        int sizeInSquareMeters,
        int capacity,
        decimal pricePerNight,
        bool isActive = true)
    {
        if (id <= 0)
            throw new ArgumentException("Room id must be greater than zero.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Room name is required.");

        if (sizeInSquareMeters <= 0)
            throw new ArgumentException("Room size must be greater than zero.");

        if (capacity <= 0)
            throw new ArgumentException("Room capacity must be greater than zero.");

        if (pricePerNight <= 0)
            throw new ArgumentException("Room price must be greater than zero.");

        Id = id;
        Name = name;
        RoomType = roomType;
        SizeInSquareMeters = sizeInSquareMeters;
        Capacity = capacity;
        PricePerNight = pricePerNight;
        IsActive = isActive;
    }

    public void UpdateDetails(
        string name,
        RoomType roomType,
        int sizeInSquareMeters,
        int capacity,
        decimal pricePerNight)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Room name is required.");

        if (sizeInSquareMeters <= 0)
            throw new ArgumentException("Room size must be greater than zero.");

        if (capacity <= 0)
            throw new ArgumentException("Room capacity must be greater than zero.");

        if (pricePerNight <= 0)
            throw new ArgumentException("Room price must be greater than zero.");

        Name = name;
        RoomType = roomType;
        SizeInSquareMeters = sizeInSquareMeters;
        Capacity = capacity;
        PricePerNight = pricePerNight;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}