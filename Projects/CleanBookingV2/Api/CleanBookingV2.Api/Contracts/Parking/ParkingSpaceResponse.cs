using CleanBookingV2.Domain.Enums;

namespace CleanBookingV2.Api.Contracts.Parking;

/// <summary>
/// Public response shape for parking space reads.
/// Parking has its own response contract because it is optional and independent
/// from room availability.
/// </summary>
public sealed record ParkingSpaceResponse(
    int Id,
    string Name,
    ParkingSpaceType ParkingSpaceType,
    bool IsActive);
