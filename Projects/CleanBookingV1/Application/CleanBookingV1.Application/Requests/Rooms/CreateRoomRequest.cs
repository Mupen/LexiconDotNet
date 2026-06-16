using CleanBookingV1.Domain.Enums;

namespace CleanBookingV1.Application.UseCases.Rooms;

public sealed record CreateRoomRequest(
    string Name,
    RoomType RoomType,
    int SizeInSquareMeters,
    int Capacity,
    decimal PricePerNight,
    bool IsActive = true);