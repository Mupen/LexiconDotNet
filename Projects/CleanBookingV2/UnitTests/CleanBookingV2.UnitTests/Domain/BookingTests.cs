using CleanBookingV2.Domain.Entities;
using CleanBookingV2.Domain.ValueObjects;

namespace CleanBookingV2.UnitTests.Domain;

/// <summary>
/// Tests Booking entity invariants directly.
/// These tests prove that invalid booking state is rejected even without controllers,
/// repositories, or the React frontend involved.
/// </summary>
public sealed class BookingTests
{
    [Fact]
    public void Constructor_CalculatesActiveBooking_WhenValuesAreValid()
    {
        // A newly created valid booking must start as active and keep the selected resources.
        var stay = new DateRange(
            new DateTime(2026, 6, 12, 14, 0, 0),
            new DateTime(2026, 6, 14, 12, 0, 0));

        var booking = new Booking(
            Guid.NewGuid(),
            "Pernille Guest",
            stay,
            2,
            2,
            1,
            1400m,
            null);

        Assert.True(booking.IsActive());
        Assert.Equal(2, booking.RoomId);
        Assert.Equal(1, booking.ParkingSpaceId);
    }

    [Fact]
    public void Constructor_AllowsCheckInAtFourteen()
    {
        // 14:00 is the earliest valid check-in time and must be accepted.
        var stay = new DateRange(
            new DateTime(2026, 6, 12, 14, 0, 0),
            new DateTime(2026, 6, 13, 12, 0, 0));

        var booking = new Booking(Guid.NewGuid(), "Guest", stay, 1, 1, null, 550m, null);

        Assert.True(booking.IsActive());
    }

    [Fact]
    public void Constructor_AllowsCheckInAtTwentyTwoThirty()
    {
        // 22:30 is the latest valid check-in time. It is late, so arrival estimate is required.
        var stay = new DateRange(
            new DateTime(2026, 6, 12, 22, 30, 0),
            new DateTime(2026, 6, 13, 12, 0, 0));

        var booking = new Booking(Guid.NewGuid(), "Guest", stay, 1, 1, null, 550m, new TimeOnly(22, 30));

        Assert.True(booking.IsActive());
    }

    [Fact]
    public void Constructor_Throws_WhenCheckInIsBeforeFourteen()
    {
        // The B&B does not allow arrivals before the check-in window opens.
        var stay = new DateRange(
            new DateTime(2026, 6, 12, 13, 59, 0),
            new DateTime(2026, 6, 13, 12, 0, 0));

        Assert.Throws<ArgumentException>(() =>
            new Booking(Guid.NewGuid(), "Guest", stay, 1, 1, null, 550m, null));
    }

    [Fact]
    public void Constructor_Throws_WhenCheckInIsAfterTwentyTwoThirty()
    {
        // 22:30 is inclusive, but anything later is outside the allowed check-in window.
        var stay = new DateRange(
            new DateTime(2026, 6, 12, 22, 31, 0),
            new DateTime(2026, 6, 13, 12, 0, 0));

        Assert.Throws<ArgumentException>(() =>
            new Booking(Guid.NewGuid(), "Guest", stay, 1, 1, null, 550m, new TimeOnly(22, 31)));
    }

    [Fact]
    public void Constructor_AllowsCheckOutAtTwelve()
    {
        // 12:00 is the latest valid checkout time and must be accepted.
        var stay = new DateRange(
            new DateTime(2026, 6, 12, 14, 0, 0),
            new DateTime(2026, 6, 13, 12, 0, 0));

        var booking = new Booking(Guid.NewGuid(), "Guest", stay, 1, 1, null, 550m, null);

        Assert.True(booking.IsActive());
    }

    [Fact]
    public void Constructor_Throws_WhenCheckOutIsAfterTwelve()
    {
        // Guests must leave no later than noon so the room can be prepared for the next stay.
        var stay = new DateRange(
            new DateTime(2026, 6, 12, 14, 0, 0),
            new DateTime(2026, 6, 13, 12, 1, 0));

        Assert.Throws<ArgumentException>(() =>
            new Booking(Guid.NewGuid(), "Guest", stay, 1, 1, null, 550m, null));
    }

    [Fact]
    public void Constructor_Throws_WhenLateArrivalHasNoEstimate()
    {
        // Arrivals after 20:00 are allowed only when the guest gives an estimated arrival time.
        var stay = new DateRange(
            new DateTime(2026, 6, 12, 20, 30, 0),
            new DateTime(2026, 6, 13, 12, 0, 0));

        Assert.Throws<ArgumentException>(() =>
            new Booking(Guid.NewGuid(), "Guest", stay, 1, 1, null, 550m, null));
    }

    [Fact]
    public void OverlapsRoom_ReturnsFalse_WhenBookingIsCancelled()
    {
        // Cancelled bookings are historical records and must not block future availability.
        var stay = new DateRange(
            new DateTime(2026, 6, 12, 14, 0, 0),
            new DateTime(2026, 6, 14, 12, 0, 0));

        var booking = new Booking(Guid.NewGuid(), "Guest", stay, 1, 1, null, 1100m, null);
        booking.Cancel();

        Assert.False(booking.OverlapsRoom(1, stay));
    }

    [Fact]
    public void UpdateDetails_Throws_WhenBookingIsCancelled()
    {
        // A cancelled booking should not be edited back into active business data.
        var stay = new DateRange(
            new DateTime(2026, 6, 12, 14, 0, 0),
            new DateTime(2026, 6, 14, 12, 0, 0));
        var booking = new Booking(Guid.NewGuid(), "Guest", stay, 1, 1, null, 1100m, null);
        booking.Cancel();

        Assert.Throws<ArgumentException>(() =>
            booking.UpdateDetails("Updated", stay, 1, 1, null, 1100m, null));
    }
}
