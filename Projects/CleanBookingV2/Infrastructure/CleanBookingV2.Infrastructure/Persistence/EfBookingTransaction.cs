using CleanBookingV2.Application.Interfaces;
using CleanBookingV2.Domain.Contracts;
using Microsoft.EntityFrameworkCore;

namespace CleanBookingV2.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of the booking transaction abstraction.
/// The application layer asks for a transaction without knowing about DbContext.
/// This implementation uses SQLite transactions to group availability checks and
/// saves into one unit of work.
/// </summary>
public sealed class EfBookingTransaction : IBookingTransaction
{
    private readonly CleanBookingV2DbContext _dbContext;

    public EfBookingTransaction(CleanBookingV2DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Runs a value-returning operation inside a database transaction.
    /// The transaction commits only when the operation returns success; failed
    /// Results leave the transaction uncommitted so no partial booking changes are
    /// persisted.
    /// </summary>
    public async Task<Result<T>> ExecuteAsync<T>(
        Func<CancellationToken, Task<Result<T>>> operation,
        CancellationToken cancellationToken)
    {
        // Keeps availability re-check and save in one unit of work. This is enough
        // for the SQLite demo, but high-concurrency production booking systems
        // usually add database-specific locking or exclusion constraints as well.
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        Result<T> result = await operation(cancellationToken);

        if (result.IsSuccess)
            await transaction.CommitAsync(cancellationToken);

        return result;
    }

    /// <summary>
    /// Runs a non-value operation inside a database transaction.
    /// This overload exists so use cases like update/cancel can use the same
    /// transaction pattern without wrapping their success result in a dummy value.
    /// </summary>
    public async Task<Result> ExecuteAsync(
        Func<CancellationToken, Task<Result>> operation,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        Result result = await operation(cancellationToken);

        if (result.IsSuccess)
            await transaction.CommitAsync(cancellationToken);

        return result;
    }
}
