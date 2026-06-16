using CleanBookingV1.Application.Contracts;
using CleanBookingV1.Application.Interfaces;
using CleanBookingV1.Application.Requests.Bookings;

namespace CleanBookingV1.Application.UseCases.Bookings;

public sealed class DeleteBooking
{
    private readonly IBookingRepository _bookingRepository;

    public DeleteBooking(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository
            ?? throw new ArgumentNullException(nameof(bookingRepository));
    }

    public Result Execute(DeleteBookingRequest request)
    {
        if (request is null)
        {
            return Result.Failure(
                new Error("Booking.Delete.NullRequest", "Request cannot be null."));
        }

        if (request.BookingId == Guid.Empty)
        {
            return Result.Failure(
                new Error("Booking.Delete.InvalidId", "Booking id cannot be empty."));
        }

        var booking = _bookingRepository.GetById(request.BookingId);

        if (booking is null)
        {
            return Result.Failure(
                new Error("Booking.Delete.NotFound", "The booking was not found."));
        }

        _bookingRepository.Delete(request.BookingId);

        return Result.Success();
    }
}