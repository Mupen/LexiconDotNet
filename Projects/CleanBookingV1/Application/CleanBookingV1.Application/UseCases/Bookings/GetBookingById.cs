using CleanBookingV1.Application.Contracts;
using CleanBookingV1.Application.Interfaces;
using CleanBookingV1.Application.Requests.Bookings;
using CleanBookingV1.Domain.Entities;

namespace CleanBookingV1.Application.UseCases.Bookings;

public sealed class GetBookingById
{
    private readonly IBookingRepository _bookingRepository;

    public GetBookingById(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository
            ?? throw new ArgumentNullException(nameof(bookingRepository));
    }

    public Result<Booking> Execute(GetBookingByIdRequest request)
    {
        if (request is null)
        {
            return Result<Booking>.Failure(
                new Error("Booking.GetById.NullRequest", "Request cannot be null."));
        }

        if (request.BookingId == Guid.Empty)
        {
            return Result<Booking>.Failure(
                new Error("Booking.GetById.InvalidId", "Booking id cannot be empty."));
        }

        var booking = _bookingRepository.GetById(request.BookingId);

        if (booking is null)
        {
            return Result<Booking>.Failure(
                new Error("Booking.GetById.NotFound", "The booking was not found."));
        }

        return Result<Booking>.Success(booking);
    }
}