using CleanBookingV2.Application.Exceptions;
using CleanBookingV2.Application.Interfaces;
using CleanBookingV2.Application.Requests.Bookings;
using CleanBookingV2.Application.Services;
using CleanBookingV2.Domain.Contracts;
using CleanBookingV2.Domain.Entities;

namespace CleanBookingV2.Application.UseCases.Bookings;

/// <summary>
/// Coordinates the complete create-booking workflow.
/// The use case is intentionally separate from the API controller so the business
/// workflow can be tested without HTTP. It also keeps the domain entity focused on
/// invariants while this class handles repositories, transactions, and persistence.
/// </summary>
public sealed class CreateBooking
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly BookingPreparationService _bookingPreparationService;
    private readonly IBookingTransaction _bookingTransaction;

    public CreateBooking(
        IBookingRepository bookingRepository,
        IGuidGenerator guidGenerator,
        BookingPreparationService bookingPreparationService,
        IBookingTransaction bookingTransaction)
    {
        _bookingRepository = bookingRepository;
        _guidGenerator = guidGenerator;
        _bookingPreparationService = bookingPreparationService;
        _bookingTransaction = bookingTransaction;
    }

    /// <summary>
    /// Creates a booking after backend validation, availability checking, and price calculation.
    /// The operation runs inside a transaction so the availability check and save
    /// happen in one unit of work. SQLite still has limits under heavy concurrency,
    /// but this is a practical protection for the learning/demo scope.
    /// </summary>
    public async Task<Result<Booking>> ExecuteAsync(CreateBookingRequest request, CancellationToken cancellationToken)
    {
        return await _bookingTransaction.ExecuteAsync(async transactionCancellationToken =>
        {
            // PrepareAsync re-loads authoritative backend data and calculates the
            // final price. The frontend request is treated as input, not truth.
            Result<BookingPreparation> preparationResult = await _bookingPreparationService.PrepareAsync(
                request.CheckIn,
                request.CheckOut,
                request.NumberOfGuests,
                request.RoomId,
                request.ParkingSpaceId,
                null,
                transactionCancellationToken);

            if (preparationResult.IsFailure)
                return Result<Booking>.Failure(preparationResult.Error!);

            BookingPreparation preparation = preparationResult.Value!;

            try
            {
                // The entity constructor enforces booking-owned invariants. This
                // protects the domain even if a future caller bypasses the API.
                var booking = new Booking(
                    _guidGenerator.NewGuid(),
                    request.GuestName,
                    preparation.Stay,
                    request.NumberOfGuests,
                    preparation.Room.Id,
                    preparation.ParkingSpaceId,
                    preparation.TotalPrice,
                    request.EstimatedArrivalTime);

                await _bookingRepository.AddAsync(booking, transactionCancellationToken);
                await _bookingRepository.SaveChangesAsync(transactionCancellationToken);
                return Result<Booking>.Success(booking);
            }
            catch (ArgumentException exception)
            {
                // ArgumentException here represents a domain validation failure,
                // so it is converted into a Result instead of becoming a 500 error.
                return Result<Booking>.Failure(new Error("Booking.Invalid", exception.Message));
            }
            catch (BookingConcurrencyException)
            {
                // EF concurrency failures are translated to a business-level conflict
                // so the API can return a controlled 409 ProblemDetails response.
                return Result<Booking>.Failure(new Error("Booking.ConcurrencyConflict", "The booking could not be saved because related booking data changed. Reload and try again."));
            }
        }, cancellationToken);
    }
}
