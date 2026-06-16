namespace CleanBookingV2.Api.Contracts.BookingPolicy;

/// <summary>
/// Public response shape for backend-owned booking policy.
/// Times are strings in HH:mm format so the React app can use them directly in
/// form controls while the backend keeps TimeOnly in the domain policy.
/// </summary>
public sealed record BookingPolicyResponse(
    string EarliestCheckIn,
    string LatestCheckIn,
    string LatestCheckOut,
    string LateArrivalThreshold,
    int TimeSlotMinutes,
    int MaximumGuests);
