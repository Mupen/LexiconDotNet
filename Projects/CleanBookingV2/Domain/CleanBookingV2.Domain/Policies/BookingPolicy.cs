namespace CleanBookingV2.Domain.Policies;

/// <summary>
/// Defines booking rules that are shared by backend validation and frontend guidance.
/// The domain owns these values because they are business policy, not UI decoration.
/// The API can expose them so React can build dropdowns and hints, but the backend
/// still validates every request. This avoids the weaker alternative of hardcoding
/// separate rule values in C# and JavaScript, where they would eventually drift.
/// </summary>
public sealed record BookingPolicy(
    TimeOnly EarliestCheckIn,
    TimeOnly LatestCheckIn,
    TimeOnly LatestCheckOut,
    TimeOnly LateArrivalThreshold,
    int TimeSlotMinutes,
    int MaximumGuests)
{
    /// <summary>
    /// Provides the active global booking policy for this project.
    /// A static policy is intentionally simple for a learning/demo application where
    /// the rules do not vary by tenant, season, or room. If those requirements arrive,
    /// callers can keep using the BookingPolicy concept while the source changes to
    /// configuration or database-backed policy.
    /// </summary>
    public static BookingPolicy Current { get; } = new(
        new TimeOnly(14, 0),
        new TimeOnly(22, 30),
        new TimeOnly(12, 0),
        new TimeOnly(20, 0),
        30,
        3);
}
