using CleanBookingV2.Domain.Policies;

namespace CleanBookingV2.UnitTests.Domain;

/// <summary>
/// Tests the backend-owned booking policy constants.
/// This protects the API/frontend policy contract from accidental changes and makes
/// the business meaning of the current time windows visible in test output.
/// </summary>
public sealed class BookingPolicyTests
{
    /// <summary>
    /// Verifies the active policy values used by domain validation and the policy endpoint.
    /// The test is intentionally explicit rather than data-driven because each value
    /// is part of the business rule documentation for the project.
    /// </summary>
    [Fact]
    public void Current_ReturnsAuthoritativeBookingRules()
    {
        BookingPolicy policy = BookingPolicy.Current;

        Assert.Equal(new TimeOnly(14, 0), policy.EarliestCheckIn);
        Assert.Equal(new TimeOnly(22, 30), policy.LatestCheckIn);
        Assert.Equal(new TimeOnly(12, 0), policy.LatestCheckOut);
        Assert.Equal(new TimeOnly(20, 0), policy.LateArrivalThreshold);
        Assert.Equal(30, policy.TimeSlotMinutes);
        Assert.Equal(3, policy.MaximumGuests);
    }
}
