namespace CleanBookingV2.Domain.Contracts;

/// <summary>
/// Represents success or failure for operations that do not return a value.
/// Expected business failures are modeled as Result values instead of exceptions,
/// because failures like "booking not found" or "room unavailable" are normal
/// outcomes that the API should map into controlled HTTP responses.
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error? Error { get; }

    protected Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Creates a successful result.
    /// A factory method keeps call sites expressive and prevents callers from
    /// constructing inconsistent success/error combinations directly.
    /// </summary>
    public static Result Success()
    {
        return new Result(true, null);
    }

    /// <summary>
    /// Creates a failed result with a specific error.
    /// The application layer can return this without knowing about HTTP status
    /// codes, which keeps transport concerns in the API project.
    /// </summary>
    public static Result Failure(Error error)
    {
        return new Result(false, error);
    }
}
