using CleanBookingV2.Application.Exceptions;
using CleanBookingV2.Application.Interfaces;
using CleanBookingV2.Domain.Contracts;

namespace CleanBookingV2.Application.UseCases.Bookings;

/// <summary>
/// Coordinates soft-cancellation of a booking.
/// Cancellation is a use case instead of a repository delete because the business
/// rule is "keep history but stop blocking availability", not "remove this row".
/// </summary>
public sealed class CancelBooking
{
    private readonly IBookingRepository _bookingRepository;

    public CancelBooking(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    /// <summary>
    /// Marks an existing booking as cancelled and saves the change.
    /// The domain entity owns the status transition, while the use case handles
    /// loading, persistence, and translating concurrency failures.
    /// </summary>
    public async Task<Result> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(id, cancellationToken);
        if (booking is null)
            return Result.Failure(new Error("Booking.NotFound", "The booking was not found."));

        booking.Cancel();
        try
        {
            await _bookingRepository.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BookingConcurrencyException)
        {
            return Result.Failure(new Error("Booking.ConcurrencyConflict", "The booking changed before it could be cancelled. Reload and try again."));
        }
    }
}
