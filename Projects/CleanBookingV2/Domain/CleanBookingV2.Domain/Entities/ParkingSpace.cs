using CleanBookingV2.Domain.Enums;

namespace CleanBookingV2.Domain.Entities;

/// <summary>
/// Represents a bookable parking space.
/// Parking is modeled separately from rooms because it has its own availability
/// timeline and can be optional on a booking. This keeps room booking rules and
/// parking assignment rules independent while using the same overlap concept.
/// </summary>
public sealed class ParkingSpace
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public ParkingSpaceType ParkingSpaceType { get; private set; }
    public bool IsActive { get; private set; }

    private ParkingSpace()
    {
    }

    /// <summary>
    /// Creates a parking space with validated identity and display data.
    /// The entity prevents invalid ids and blank names at the boundary where parking
    /// spaces are created, instead of relying on every caller to remember those
    /// checks.
    /// </summary>
    public ParkingSpace(int id, string name, ParkingSpaceType parkingSpaceType, bool isActive = true)
    {
        if (id <= 0)
            throw new ArgumentException("Parking space id must be greater than zero.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Parking space name is required.");

        Id = id;
        Name = name.Trim();
        ParkingSpaceType = parkingSpaceType;
        IsActive = isActive;
    }
}
