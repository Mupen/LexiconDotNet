namespace CleanBookingV2.Domain.ValueObjects;

/// <summary>
/// Represents a stay interval from check-in to check-out.
/// This is modeled as a value object because it has no identity; two ranges with
/// the same start and end mean the same thing. Keeping overlap and night-count
/// behavior here prevents controllers, queries, and use cases from duplicating
/// slightly different date logic.
/// </summary>
public sealed class DateRange
{
    public DateTime Start { get; private set; }
    public DateTime End { get; private set; }

    private DateRange()
    {
    }

    /// <summary>
    /// Creates a valid positive date range.
    /// Invalid ranges are rejected at construction time so the rest of the domain
    /// can trust DateRange instances. This is preferable to carrying invalid dates
    /// through the system and checking them repeatedly in every use case.
    /// </summary>
    public DateRange(DateTime start, DateTime end)
    {
        if (end <= start)
        {
            throw new ArgumentException("Check-out must be after check-in.");
        }

        Start = start;
        End = end;
    }

    /// <summary>
    /// Calculates chargeable nights using calendar-date difference.
    /// The project charges by room night rather than exact hours, so checkout time
    /// does not add fractional pricing. Math.Max protects callers from a zero-night
    /// value if a very short valid range ever reaches this method.
    /// </summary>
    public int GetNumberOfNights()
    {
        return Math.Max(1, (End.Date - Start.Date).Days);
    }

    /// <summary>
    /// Checks whether this range and another range share any time.
    /// The half-open interval formula allows back-to-back bookings because a stay
    /// ending before the next begins should not block the next guest.
    /// </summary>
    public bool Overlaps(DateRange other)
    {
        return Start < other.End && other.Start < End;
    }
}
