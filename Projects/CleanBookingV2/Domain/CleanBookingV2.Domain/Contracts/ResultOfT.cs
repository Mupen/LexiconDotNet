namespace CleanBookingV2.Domain.Contracts;

/// <summary>
/// Represents success or failure for operations that return a value.
/// It builds on Result so all use cases use the same success/failure pattern,
/// whether they return data or only report completion. Value is meaningful only
/// when IsSuccess is true.
/// </summary>
public sealed class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool isSuccess, T? value, Error? error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a successful result carrying a value.
    /// The factory keeps valid construction paths obvious and avoids exposing the
    /// internal constructor to application code.
    /// </summary>
    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value, null);
    }

    /// <summary>
    /// Creates a failed result with no value.
    /// The error explains the failure, so callers do not need placeholder objects
    /// for unsuccessful operations.
    /// </summary>
    public new static Result<T> Failure(Error error)
    {
        return new Result<T>(false, default, error);
    }
}
