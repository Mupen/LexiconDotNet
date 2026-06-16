using CleanBookingV2.Application.Exceptions;
using CleanBookingV2.Application.Interfaces;
using CleanBookingV2.Application.Requests.Bookings;
using CleanBookingV2.Application.Services;
using CleanBookingV2.Domain.Contracts;

namespace CleanBookingV2.Application.UseCases.Bookings;

/// <summary>
/// Coordinates edits to an existing booking.
/// Update reuses the same preparation service as create because changing dates,
/// room, guest count, or parking must re-check availability and recalculate price.
/// </summary>
public sealed class UpdateBooking
{
    private readonly IBookingRepository _bookingRepository;
    private readonly BookingPreparationService _bookingPreparationService;
    private readonly IBookingTransaction _bookingTransaction;

    public UpdateBooking(
        IBookingRepository bookingRepository,
        BookingPreparationService bookingPreparationService,
        IBookingTransaction bookingTransaction)
    {
        _bookingRepository = bookingRepository;
        _bookingPreparationService = bookingPreparationService;
        _bookingTransaction = bookingTransaction;
    }

    /// <summary>
    /// Updates a booking if it exists and the requested new resources are valid.
    /// The current booking id is ignored during availability checks so keeping the
    /// same room/parking/date range does not conflict with itself.
    /// </summary>
    public async Task<Result> ExecuteAsync(UpdateBookingRequest request, CancellationToken cancellationToken)
    {
        return await _bookingTransaction.ExecuteAsync(async transactionCancellationToken =>
        {
            // A tracked entity is loaded because update mutates the domain object and
            // then relies on EF Core change tracking to persist the modifications.
            var booking = await _bookingRepository.GetByIdAsync(request.Id, transactionCancellationToken);
            if (booking is null)
                return Result.Failure(new Error("Booking.NotFound", "The booking was not found."));

            Result<BookingPreparation> preparationResult = await _bookingPreparationService.PrepareAsync(
                request.CheckIn,
                request.CheckOut,
                request.NumberOfGuests,
                request.RoomId,
                request.ParkingSpaceId,
                booking.Id,
                transactionCancellationToken);

            if (preparationResult.IsFailure)
                return Result.Failure(preparationResult.Error!);

            BookingPreparation preparation = preparationResult.Value!;

            try
            {
                // The entity performs final invariant checks, including preventing
                // updates to cancelled bookings.
                booking.UpdateDetails(
                    request.GuestName,
                    preparation.Stay,
                    request.NumberOfGuests,
                    preparation.Room.Id,
                    preparation.ParkingSpaceId,
                    preparation.TotalPrice,
                    request.EstimatedArrivalTime);

                await _bookingRepository.SaveChangesAsync(transactionCancellationToken);
                return Result.Success();
            }
            catch (ArgumentException exception)
            {
                return Result.Failure(new Error("Booking.Invalid", exception.Message));
            }
            catch (BookingConcurrencyException)
            {
                // A concurrency conflict means another save changed the booking after
                // it was loaded. Returning Result keeps this as a controlled business
                // failure instead of an unhandled infrastructure exception.
                return Result.Failure(new Error("Booking.ConcurrencyConflict", "The booking changed while you were editing it. Reload and try again."));
            }
        }, cancellationToken);
    }
}
