using System.ComponentModel.DataAnnotations;

namespace CleanBookingV2.Api.Contracts.Bookings;

/// <summary>
/// HTTP request body for updating a booking.
/// It mirrors create input because update recalculates the same backend-owned
/// availability and pricing decisions from the submitted working state.
/// </summary>
public sealed record UpdateBookingDto(
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
    /// Performs DTO-level validation for required date values.
    /// More complex rules, such as room capacity and overlap checks, remain in the
    /// application/domain layers where backend state is available.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CheckIn == default)
            yield return new ValidationResult("Check-in is required.", [nameof(CheckIn)]);

        if (CheckOut == default)
            yield return new ValidationResult("Check-out is required.", [nameof(CheckOut)]);
    }
}
