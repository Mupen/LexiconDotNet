using ReactNews.Api.Contracts.Common;
using ReactNews.Application.Contracts.Common;

namespace ReactNews.Api.Mapping.Common;

/// <summary>
/// Converts application Result values into HTTP responses.
/// </summary>
/// <remarks>
/// What: maps successful use case results to 200 OK and validation failures to
/// 400 Bad Request.
/// Why: this keeps controllers thin and prevents every action from hand-writing
/// the same Result handling logic.
/// How: controllers call ToHttpResult after a use case returns Result&lt;T&gt;.
/// </remarks>
public static class ApiResultMapping
{
    /// <summary>
    /// Maps an application Result&lt;T&gt; into an HTTP result.
    /// </summary>
    /// <remarks>
    /// What: returns 200 OK for successful use case values and 400 Bad Request
    /// for expected validation/application failures.
    /// How: Result.IsSuccess controls the branch; missing error details fall back
    /// to a generic error response.
    /// Why: controllers should not duplicate this success/failure pattern, and
    /// frontend code should receive stable error shapes.
    /// </remarks>
    public static IResult ToHttpResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        var error = result.Error ?? new Error("unknown_error", "The request could not be completed.");

        return error.Code == "not_found"
            ? Results.NotFound(new ErrorResponse(error.Code, error.Message))
            : Results.BadRequest(new ErrorResponse(error.Code, error.Message));
    }
}
