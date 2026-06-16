using CleanBookingV2.Api.Contracts.Rooms;
using CleanBookingV2.Api.Mapping;
using CleanBookingV2.Application.Queries.Rooms;
using Microsoft.AspNetCore.Mvc;

namespace CleanBookingV2.Api.Controllers;

/// <summary>
/// HTTP API for room catalog and room availability reads.
/// Room data is read-only in this project, so the controller exposes query endpoints
/// without create/update/delete actions.
/// </summary>
[ApiController]
[Route("api/rooms")]
public sealed class RoomsController : ControllerBase
{
    /// <summary>
    /// Returns all rooms for browsing.
    /// The backend returns authoritative capacity and price values; the frontend
    /// should display these values rather than inventing its own.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RoomResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromServices] GetAllRooms query, CancellationToken cancellationToken)
    {
        var rooms = await query.ExecuteAsync(cancellationToken);
        return Ok(rooms.Select(room => room.ToResponse()).ToList());
    }

    /// <summary>
    /// Returns rooms available for a date range and guest count.
    /// This is useful frontend working state, but saving a booking still re-checks
    /// availability because search results can become stale.
    /// </summary>
    [HttpGet("available")]
    [ProducesResponseType(typeof(IReadOnlyList<RoomResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAvailable(
        [FromQuery] DateTime checkIn,
        [FromQuery] DateTime checkOut,
        [FromQuery] int guests,
        [FromServices] GetAvailableRooms query,
        CancellationToken cancellationToken)
    {
        if (guests <= 0)
            return Problem(title: "Rooms.InvalidAvailabilityRequest", detail: "Guests must be greater than zero.", statusCode: StatusCodes.Status400BadRequest);

        if (checkIn == default || checkOut == default)
            return Problem(title: "Rooms.InvalidAvailabilityRequest", detail: "Check-in and check-out are required.", statusCode: StatusCodes.Status400BadRequest);

        try
        {
            var rooms = await query.ExecuteAsync(checkIn, checkOut, guests, cancellationToken);
            return Ok(rooms.Select(room => room.ToResponse()).ToList());
        }
        catch (ArgumentException exception)
        {
            return Problem(title: "Rooms.InvalidAvailabilityRequest", detail: exception.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }
}
