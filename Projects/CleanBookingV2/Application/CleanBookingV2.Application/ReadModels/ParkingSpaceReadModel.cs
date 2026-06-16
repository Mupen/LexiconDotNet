using CleanBookingV2.Domain.Enums;

namespace CleanBookingV2.Application.ReadModels;

/// <summary>
/// Represents parking space data shaped for API reads.
/// The application returns this simple projection instead of exposing domain
/// entities directly, keeping the API response contract stable.
/// </summary>
public sealed record ParkingSpaceReadModel(
    int Id,
    string Name,
    ParkingSpaceType ParkingSpaceType,
    bool IsActive);
