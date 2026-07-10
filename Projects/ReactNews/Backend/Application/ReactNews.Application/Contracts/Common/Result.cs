namespace ReactNews.Application.Contracts.Common;

/// <summary>
/// Represents either a successful value or an expected application error.
/// </summary>
/// <remarks>
/// What: Result&lt;T&gt; gives use cases an explicit success/failure return type.
/// Why: it makes expected failures visible in method signatures and reduces
/// controller catch blocks for normal validation problems.
/// How: use cases return Success(value) when work completes, or Failure(error)
/// when the request is invalid or cannot be satisfied for a known reason.
/// </remarks>
public sealed class Result<T>
{
    private Result(T? value, Error? error, bool isSuccess)
    {
        Value = value;
        Error = error;
        IsSuccess = isSuccess;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public T? Value { get; }

    public Error? Error { get; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <remarks>
    /// What: wraps a value that a use case produced successfully.
    /// How: Value is set, Error is null, and IsSuccess is true.
    /// Why: callers can handle success and failure through one return type rather
    /// than mixing normal values with exceptions for expected outcomes.
    /// </remarks>
    public static Result<T> Success(T value)
    {
        return new Result<T>(value, null, true);
    }

    /// <summary>
    /// Creates a failed result for an expected application error.
    /// </summary>
    /// <remarks>
    /// What: wraps an Error without a value.
    /// How: Value is default, Error is set, and IsSuccess is false.
    /// Why: validation and business-rule failures are normal application flow and
    /// should be explicit without throwing exceptions.
    /// </remarks>
    public static Result<T> Failure(Error error)
    {
        return new Result<T>(default, error, false);
    }
}
