using CleanBookingV1.Domain.Enums;

namespace CleanBookingV1.Application.Requests.Parking;

public sealed record UpdateParkingSpaceRequest(
    int ParkingSpaceId,
    string Name,
    ParkingSpaceType Type);