using CleanBookingV1.Application.Contracts;
using CleanBookingV1.Application.Interfaces;
using CleanBookingV1.Domain.Entities;

namespace CleanBookingV1.Application.UseCases.Bookings;

public sealed class GetAllBookings
{
    private readonly IBookingRepository _bookingRepository;

    public GetAllBookings(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository
            ?? throw new ArgumentNullException(nameof(bookingRepository));
    }

    public Result<IReadOnlyList<Booking>> Execute()
    {
        var bookings = _bookingRepository.GetAll();

        return Result<IReadOnlyList<Booking>>.Success(bookings);
    }
}