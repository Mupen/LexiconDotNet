namespace ReactNews.Application.Contracts.Common;

/// <summary>
/// Represents an expected application failure.
/// </summary>
/// <remarks>
/// What: an Error contains a stable code and a human-readable message.
/// Why: expected failures such as invalid search input should not have to travel
/// through the system as exceptions. Exceptions are still useful for unexpected
/// infrastructure failures, but validation is normal control flow.
/// How: use cases return Result&lt;T&gt; with an Error when the request cannot be
/// completed. The API layer maps that result into a proper HTTP response.
/// </remarks>
public sealed record Error(string Code, string Message)
{
    /// <summary>
    /// Creates a validation error.
    /// </summary>
    /// <remarks>
    /// What: gives invalid request input a consistent error code.
    /// How: the caller supplies the message and the code is fixed to
    /// validation_error.
    /// Why: stable codes are easier for APIs/tests/frontend logic to handle than
    /// parsing free-text messages.
    /// </remarks>
    public static Error Validation(string message)
    {
        return new Error("validation_error", message);
    }

    /// <summary>
    /// Creates a not-found error.
    /// </summary>
    /// <remarks>
    /// What: gives missing resources a consistent application error code.
    /// How: the caller supplies the readable message and the code is fixed to
    /// not_found.
    /// Why: controllers can map missing saved articles or snapshots to HTTP 404
    /// without throwing exceptions for expected user flow.
    /// </remarks>
    public static Error NotFound(string message)
    {
        return new Error("not_found", message);
    }
}
