using CleanBookingV2.Domain.Contracts;

namespace CleanBookingV2.Application.Interfaces;

/// <summary>
/// Wraps booking workflows in a persistence transaction.
/// The application layer depends on this abstraction instead of EF Core directly,
/// allowing use cases to express "availability check plus save is one unit" without
/// knowing how SQLite transactions are implemented.
/// </summary>
public interface IBookingTransaction
{
    Task<Result<T>> ExecuteAsync<T>(
        Func<CancellationToken, Task<Result<T>>> operation,
        CancellationToken cancellationToken);

    Task<Result> ExecuteAsync(
        Func<CancellationToken, Task<Result>> operation,
        CancellationToken cancellationToken);
}
