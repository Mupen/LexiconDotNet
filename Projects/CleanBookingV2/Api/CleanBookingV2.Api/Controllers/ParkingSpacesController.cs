using CleanBookingV2.Api.Contracts.Parking;
using CleanBookingV2.Api.Mapping;
using CleanBookingV2.Application.Queries.Parking;
using Microsoft.AspNetCore.Mvc;

namespace CleanBookingV2.Api.Controllers;

/// <summary>
/// HTTP API for parking space catalog and availability reads.
/// Parking is optional on bookings, so availability is exposed separately from room
/// availability rather than being hidden inside room results.
/// </summary>
[ApiController]
[Route("api/parking-spaces")]
public sealed class ParkingSpacesController : ControllerBase
{
    /// <summary>
    /// Returns all parking spaces for display.
    /// This endpoint gives the frontend backend-owned parking names and types.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ParkingSpaceResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromServices] GetAllParkingSpaces query, CancellationToken cancellationToken)
    {
        var spaces = await query.ExecuteAsync(cancellationToken);
        return Ok(spaces.Select(space => space.ToResponse()).ToList());
    }

    /// <summary>
    /// Returns parking spaces available for the requested stay.
    /// The query result is only a snapshot for the UI; create/update commands still
    /// perform authoritative backend validation.
    /// </summary>
    [HttpGet("available")]
    [ProducesResponseType(typeof(IReadOnlyList<ParkingSpaceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAvailable(
        [FromQuery] DateTime checkIn,
        [FromQuery] DateTime checkOut,
        [FromServices] GetAvailableParkingSpaces query,
        CancellationToken cancellationToken)
    {
        if (checkIn == default || checkOut == default)
            return Problem(title: "Parking.InvalidAvailabilityRequest", detail: "Check-in and check-out are required.", statusCode: StatusCodes.Status400BadRequest);

        try
        {
            var spaces = await query.ExecuteAsync(checkIn, checkOut, cancellationToken);
            return Ok(spaces.Select(space => space.ToResponse()).ToList());
        }
        catch (ArgumentException exception)
        {
            return Problem(title: "Parking.InvalidAvailabilityRequest", detail: exception.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }
}
