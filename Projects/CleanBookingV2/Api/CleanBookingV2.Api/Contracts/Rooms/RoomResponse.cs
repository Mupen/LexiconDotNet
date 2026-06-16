using CleanBookingV2.Domain.Enums;

namespace CleanBookingV2.Api.Contracts.Rooms;

/// <summary>
/// Public response shape for room reads.
/// The frontend uses this backend-owned data for room display and selection; it
/// should not invent capacity or pricing locally.
/// </summary>
public sealed record RoomResponse(
    int Id,
    string Name,
    RoomType RoomType,
    int SizeInSquareMeters,
    int Capacity,
    decimal PricePerNight,
    bool IsActive);
