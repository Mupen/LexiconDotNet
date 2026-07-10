using ReactNews.Application.Exceptions;
using ReactNews.Api.Contracts.Common;

namespace ReactNews.Api.Mapping.Common;

/// <summary>
/// Converts known application/infrastructure exceptions into HTTP responses.
/// </summary>
/// <remarks>
/// What: this class centralizes exception-to-status-code decisions.
/// Why: controllers should not repeat the same catch blocks differently on each
/// endpoint. Central mapping keeps behavior consistent for React.
/// How: controllers call ToProblemResult inside a catch block for expected
/// exceptions; unexpected exceptions still flow to the global exception handler.
/// </remarks>
public static class ApiExceptionMapping
{
    /// <summary>
    /// Maps known exceptions into HTTP problem/error responses.
    /// </summary>
    /// <remarks>
    /// What: converts validation/configuration/provider exceptions into status
    /// codes that match the failure category.
    /// How: switch pattern matching chooses 400, 500, or 502 responses.
    /// Why: the frontend can react more accurately when setup errors, external
    /// provider failures, and user input errors are not all treated the same.
    /// </remarks>
    public static IResult ToProblemResult(Exception exception)
    {
        return exception switch
        {
            ArgumentException argumentException => Results.BadRequest(new ErrorResponse("validation_error", argumentException.Message)),
            NewsConfigurationException configurationException => Results.Problem(configurationException.Message, statusCode: StatusCodes.Status500InternalServerError),
            NewsProviderException providerException => Results.Problem(providerException.Message, statusCode: StatusCodes.Status502BadGateway),
            _ => Results.Problem("An unexpected error occurred.", statusCode: StatusCodes.Status500InternalServerError)
        };
    }
}
