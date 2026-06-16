using CleanBookingV2.Domain.ValueObjects;

namespace CleanBookingV2.UnitTests.Domain;

/// <summary>
/// Tests DateRange behavior in isolation.
/// Date overlap rules are central to room and parking availability, so these tests
/// document the edge cases before repositories and use cases add more complexity.
/// </summary>
public sealed class DateRangeTests
{
    [Fact]
    public void Overlaps_ReturnsTrue_WhenRangesShareDates()
    {
        // Any shared time between two stays means the room or parking resource is unavailable.
        var first = new DateRange(
            new DateTime(2026, 6, 12, 14, 0, 0),
            new DateTime(2026, 6, 14, 12, 0, 0));

        var second = new DateRange(
            new DateTime(2026, 6, 13, 14, 0, 0),
            new DateTime(2026, 6, 15, 12, 0, 0));

        Assert.True(first.Overlaps(second));
    }

    [Fact]
    public void Overlaps_ReturnsFalse_WhenSecondStayStartsAtFirstCheckout()
    {
        // Back-to-back stays are allowed: checkout at 12:00 and next check-in at 14:00 do not overlap.
        var first = new DateRange(
            new DateTime(2026, 6, 12, 14, 0, 0),
            new DateTime(2026, 6, 14, 12, 0, 0));

        var second = new DateRange(
            new DateTime(2026, 6, 14, 14, 0, 0),
            new DateTime(2026, 6, 16, 12, 0, 0));

        Assert.False(first.Overlaps(second));
    }

    [Fact]
    public void Constructor_Throws_WhenCheckoutIsBeforeCheckin()
    {
        // A stay must have a positive duration.
        Assert.Throws<ArgumentException>(() =>
            new DateRange(
                new DateTime(2026, 6, 14, 14, 0, 0),
                new DateTime(2026, 6, 12, 12, 0, 0)));
    }

    [Fact]
    public void GetNumberOfNights_ReturnsDateDifference()
    {
        // Pricing is based on date difference, not the exact checkout hour.
        var stay = new DateRange(
            new DateTime(2026, 6, 12, 14, 0, 0),
            new DateTime(2026, 6, 15, 12, 0, 0));

        Assert.Equal(3, stay.GetNumberOfNights());
    }
}
