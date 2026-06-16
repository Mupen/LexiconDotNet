using CleanBookingV2.Application.Interfaces;
using CleanBookingV2.Application.ReadModels;

namespace CleanBookingV2.Application.Queries.Bookings;

/// <summary>
/// Reads booking summaries for display.
/// This query uses the read repository instead of the write repository because the
/// caller needs projected data such as room name, not a tracked domain entity.
/// </summary>
public sealed class GetAllBookings
{
    private readonly IBookingReadRepository _bookingReadRepository;

    public GetAllBookings(IBookingReadRepository bookingReadRepository)
    {
        _bookingReadRepository = bookingReadRepository;
    }

    /// <summary>
    /// Returns all bookings in read-model form.
    /// The application query hides persistence details from the API controller while
    /// still allowing infrastructure to optimize the actual database query.
    /// </summary>
    public async Task<IReadOnlyList<BookingReadModel>> ExecuteAsync(CancellationToken cancellationToken)
    {
        return await _bookingReadRepository.GetAllAsync(cancellationToken);
    }
}
