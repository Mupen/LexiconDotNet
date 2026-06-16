using CleanBookingV2.Domain.Entities;
using CleanBookingV2.Domain.ValueObjects;

namespace CleanBookingV2.Application.Services;

/// <summary>
/// Carries validated, backend-derived booking data into create/update use cases.
/// A record is used because this is immutable workflow data, not an entity with its
/// own identity. It keeps the use cases compact after preparation succeeds.
/// </summary>
public sealed record BookingPreparation(
    Room Room,
    DateRange Stay,
    int? ParkingSpaceId,
    decimal TotalPrice);
