using CleanBookingV2.Domain.Enums;

namespace CleanBookingV2.Application.ReadModels;

/// <summary>
/// Represents room data shaped for API reads.
/// This projection contains only the information the frontend needs for listing,
/// selection, and availability display.
/// </summary>
public sealed record RoomReadModel(
    int Id,
    string Name,
    RoomType RoomType,
    int SizeInSquareMeters,
    int Capacity,
    decimal PricePerNight,
    bool IsActive);
