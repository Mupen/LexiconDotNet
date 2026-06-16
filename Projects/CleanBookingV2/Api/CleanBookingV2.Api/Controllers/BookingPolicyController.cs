using CleanBookingV2.Api.Contracts.BookingPolicy;
using CleanBookingV2.Domain.Policies;
using Microsoft.AspNetCore.Mvc;

namespace CleanBookingV2.Api.Controllers;

/// <summary>
/// Exposes backend-owned booking policy to the frontend.
/// The frontend uses this endpoint for working-state controls such as dropdowns and
/// hints, but the backend still validates every booking command independently.
/// </summary>
[ApiController]
[Route("api/booking-policy")]
public sealed class BookingPolicyController : ControllerBase
{
    /// <summary>
    /// Returns the active booking policy in frontend-friendly string form.
    /// TimeOnly values are formatted as HH:mm because HTML time controls and the
    /// React code both work naturally with that representation.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(BookingPolicyResponse), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        BookingPolicy policy = BookingPolicy.Current;

        return Ok(new BookingPolicyResponse(
            policy.EarliestCheckIn.ToString("HH:mm"),
            policy.LatestCheckIn.ToString("HH:mm"),
            policy.LatestCheckOut.ToString("HH:mm"),
            policy.LateArrivalThreshold.ToString("HH:mm"),
            policy.TimeSlotMinutes,
            policy.MaximumGuests));
    }
}
