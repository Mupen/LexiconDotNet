using System.ComponentModel.DataAnnotations;

namespace CleanBookingV2.Api.Contracts.Bookings;

/// <summary>
/// HTTP request body for creating a booking.
/// Data annotation attributes catch simple transport-level validation before the
/// request reaches the application use case, while business rules are still checked
/// by the backend domain/application layers.
/// </summary>
public sealed record CreateBookingDto(
    [Required]
    string GuestName,
    DateTime CheckIn,
    DateTime CheckOut,
    [Range(1, 10)]
    int NumberOfGuests,
    [Range(1, int.MaxValue)]
    int RoomId,
    [Range(1, int.MaxValue)]
    int? ParkingSpaceId,
    TimeOnly? EstimatedArrivalTime) : IValidatableObject
{
    /// <summary>
    /// Performs DTO-level validation that attributes cannot express cleanly.
    /// This checks required DateTime values because default DateTime can otherwise
    /// appear as a valid struct value even when the client omitted it.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CheckIn == default)
            yield return new ValidationResult("Check-in is required.", [nameof(CheckIn)]);

        if (CheckOut == default)
            yield return new ValidationResult("Check-out is required.", [nameof(CheckOut)]);
    }
}
