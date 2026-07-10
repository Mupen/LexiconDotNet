namespace ReactNews.Application.Exceptions;

/// <summary>
/// What: Raised when the backend is missing configuration required to call NewsAPI.
/// How: Infrastructure throws this before sending an HTTP request when settings such as the API key are absent.
/// Why: Configuration mistakes should fail clearly as backend setup problems instead of looking like provider downtime or user input errors.
/// </summary>
public sealed class NewsConfigurationException : Exception
{
    /// <summary>
    /// What: Creates the configuration exception with a readable message.
    /// How: Passes the message to the base Exception type.
    /// Why: The API exception mapper can expose a controlled problem response while logs keep the original reason.
    /// </summary>
    public NewsConfigurationException(string message) : base(message)
    {
    }
}
