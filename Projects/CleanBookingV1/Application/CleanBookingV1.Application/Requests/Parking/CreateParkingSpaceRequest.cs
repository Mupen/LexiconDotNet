using CleanBookingV1.Application.Interfaces;
using CleanBookingV1.Domain.Enums;

namespace CleanBookingV1.Application.Requests.Parking;

public sealed record CreateParkingSpaceRequest(
    string Name,
    ParkingSpaceType Type,
    bool IsActive = true);

