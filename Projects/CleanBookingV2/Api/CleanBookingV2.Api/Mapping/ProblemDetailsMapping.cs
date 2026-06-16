using CleanBookingV2.Domain.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace CleanBookingV2.Api.Mapping;

/// <summary>
/// Maps application Result failures into HTTP ProblemDetails responses.
/// Keeping this mapping in one place avoids repeating status-code decisions inside
/// every controller action.
/// </summary>
public static class ProblemDetailsMapping
{
    /// <summary>
    /// Converts a failed Result into a ProblemDetails response.
    /// Error codes remain application/domain concepts; this mapper decides which
    /// HTTP status code best represents each category.
    /// </summary>
    public static IActionResult ToProblem(this ControllerBase controller, Result result)
    {
        int statusCode = result.Error?.Code switch
        {
            string code when code.Contains("NotFound", StringComparison.OrdinalIgnoreCase) => StatusCodes.Status404NotFound,
            string code when code.Contains("ConcurrencyConflict", StringComparison.OrdinalIgnoreCase) => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest
        };

        return controller.Problem(
            title: result.Error?.Code ?? "Request failed",
            detail: result.Error?.Message,
            statusCode: statusCode);
    }
}
