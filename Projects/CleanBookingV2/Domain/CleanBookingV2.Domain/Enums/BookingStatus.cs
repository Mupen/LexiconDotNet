namespace CleanBookingV2.Domain.Enums;

/// <summary>
/// Describes whether a booking currently blocks availability.
/// A status enum is used instead of deleting cancelled bookings so the system can
/// keep booking history while still excluding cancelled records from overlap checks.
/// </summary>
public enum BookingStatus
{
    Active = 1,
    Cancelled = 2
}
