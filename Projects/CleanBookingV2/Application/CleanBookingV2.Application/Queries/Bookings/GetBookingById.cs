using CleanBookingV2.Application.Interfaces;
using CleanBookingV2.Application.ReadModels;

namespace CleanBookingV2.Application.Queries.Bookings;

/// <summary>
/// Reads one booking by id for API responses.
/// This exists as a query object so controllers do not call repositories directly,
/// keeping the API layer thin and consistent with the rest of the architecture.
/// </summary>
public sealed class GetBookingById
{
    private readonly IBookingReadRepository _bookingReadRepository;

    public GetBookingById(IBookingReadRepository bookingReadRepository)
    {
        _bookingReadRepository = bookingReadRepository;
    }

    /// <summary>
    /// Returns a booking read model or null when it does not exist.
    /// Returning null keeps absence explicit so the controller can map it to 404.
    /// </summary>
    public async Task<BookingReadModel?> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _bookingReadRepository.GetByIdAsync(id, cancellationToken);
    }
}
