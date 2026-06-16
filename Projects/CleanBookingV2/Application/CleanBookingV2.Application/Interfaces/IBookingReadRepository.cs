using CleanBookingV2.Application.ReadModels;

namespace CleanBookingV2.Application.Interfaces;

/// <summary>
/// Provides read-optimized booking projections.
/// This interface is separate from IBookingRepository because reads need joined
/// display data, while writes need tracked domain entities. Splitting them keeps
/// each dependency honest about how the data will be used.
/// </summary>
public interface IBookingReadRepository
{
    Task<IReadOnlyList<BookingReadModel>> GetAllAsync(CancellationToken cancellationToken);
    Task<BookingReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
