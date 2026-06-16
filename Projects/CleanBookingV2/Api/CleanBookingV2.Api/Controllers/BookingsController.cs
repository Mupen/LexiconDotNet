using CleanBookingV2.Api.Contracts.Bookings;
using CleanBookingV2.Api.Mapping;
using CleanBookingV2.Application.Queries.Bookings;
using CleanBookingV2.Application.Requests.Bookings;
using CleanBookingV2.Application.UseCases.Bookings;
using Microsoft.AspNetCore.Mvc;

namespace CleanBookingV2.Api.Controllers;

/// <summary>
/// HTTP API for booking read and write workflows.
/// Controllers stay thin: they translate HTTP input into application requests,
/// call use cases/queries, and map application results back to HTTP responses.
/// </summary>
[ApiController]
[Route("api/bookings")]
public sealed class BookingsController : ControllerBase
{
    /// <summary>
    /// Returns all bookings for dashboard display.
    /// The query already returns read models, so the controller only maps them into
    /// API response records.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BookingResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromServices] GetAllBookings query, CancellationToken cancellationToken)
    {
        var bookings = await query.ExecuteAsync(cancellationToken);
        return Ok(bookings.Select(booking => booking.ToResponse()).ToList());
    }

    /// <summary>
    /// Returns one booking by id or 404 when absent.
    /// Missing data is handled explicitly here because absence is a normal API
    /// outcome, not an exceptional condition.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromServices] GetBookingById query, CancellationToken cancellationToken)
    {
        var booking = await query.ExecuteAsync(id, cancellationToken);
        return booking is null ? NotFound() : Ok(booking.ToResponse());
    }

    /// <summary>
    /// Creates a booking through the application use case.
    /// The controller does not calculate price or trust frontend availability; it
    /// delegates to CreateBooking so the backend remains authoritative.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        CreateBookingDto dto,
        [FromServices] CreateBooking useCase,
        [FromServices] GetBookingById getBookingById,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(
            new CreateBookingRequest(
                dto.GuestName,
                dto.CheckIn,
                dto.CheckOut,
                dto.NumberOfGuests,
                dto.RoomId,
                dto.ParkingSpaceId,
                dto.EstimatedArrivalTime),
            cancellationToken);

        if (result.IsFailure)
            return this.ToProblem(result);

        // Reload the booking through the read model query so the response includes
        // joined room and parking names, not just the domain entity fields.
        var booking = await getBookingById.ExecuteAsync(result.Value!.Id, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, booking!.ToResponse());
    }

    /// <summary>
    /// Updates an existing booking through the application use case.
    /// Route id and body values are combined into one application request so the
    /// use case receives a complete command object.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateBookingDto dto,
        [FromServices] UpdateBooking useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(
            new UpdateBookingRequest(
                id,
                dto.GuestName,
                dto.CheckIn,
                dto.CheckOut,
                dto.NumberOfGuests,
                dto.RoomId,
                dto.ParkingSpaceId,
                dto.EstimatedArrivalTime),
            cancellationToken);

        return result.IsSuccess ? NoContent() : this.ToProblem(result);
    }

    /// <summary>
    /// Cancels a booking with a soft-delete status change.
    /// DELETE is used as the HTTP verb because the booking is removed from active
    /// availability, even though the database row is kept for history.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, [FromServices] CancelBooking useCase, CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : this.ToProblem(result);
    }
}
