using CleanBookingV2.Domain.Enums;

namespace CleanBookingV2.Domain.Entities;

/// <summary>
/// Represents a rentable room in the bed and breakfast.
/// Room is a domain entity because it has a stable identity and business properties
/// such as capacity and nightly price. The entity validates its own basic data so
/// seeded or future managed rooms cannot enter the system with impossible values.
/// </summary>
public sealed class Room
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public RoomType RoomType { get; private set; }
    public int SizeInSquareMeters { get; private set; }
    public int Capacity { get; private set; }
    public decimal PricePerNight { get; private set; }
    public bool IsActive { get; private set; }

    private Room()
    {
    }

    /// <summary>
    /// Creates a room with validated descriptive and pricing data.
    /// These checks are kept in the entity instead of only in EF configuration or
    /// controllers because rooms may be constructed from tests, seed data, or future
    /// admin workflows. Central validation keeps all those paths consistent.
    /// </summary>
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
        Name = name.Trim();
        RoomType = roomType;
        SizeInSquareMeters = sizeInSquareMeters;
        Capacity = capacity;
        PricePerNight = pricePerNight;
        IsActive = isActive;
    }
}
