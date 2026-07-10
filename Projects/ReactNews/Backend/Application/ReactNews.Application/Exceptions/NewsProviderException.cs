namespace ReactNews.Application.Exceptions;

/// <summary>
/// Raised when an external news provider rejects or fails a request.
/// </summary>
/// <remarks>
/// What: this exception represents dependency failure, not a user input mistake.
/// Why: the API can map it to 502 Bad Gateway, which tells the frontend that
/// ReactNews was reachable but the upstream provider could not satisfy the call.
/// How: infrastructure throws it after inspecting the HTTP status and provider
/// response body.
/// </remarks>
public sealed class NewsProviderException : Exception
{
    /// <summary>
    /// What: Creates the provider exception with a readable message.
    /// How: Passes the message to the base Exception type.
    /// Why: Infrastructure can translate raw provider failures into one application-level exception type.
    /// </summary>
    public NewsProviderException(string message) : base(message)
    {
    }
}
