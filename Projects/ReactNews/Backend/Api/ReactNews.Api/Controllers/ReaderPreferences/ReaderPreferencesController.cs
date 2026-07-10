using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ReactNews.Api.Mapping.Common;
using ReactNews.Application.Contracts.ReaderPreferences;
using ReactNews.Application.Queries.ReaderPreferences;

namespace ReactNews.Api.Controllers.ReaderPreferences;

/// <summary>
/// What: HTTP endpoints for the signed-in reader's preferences.
/// How: Delegates reading and updating to Application use cases.
/// Why: The API should expose reader intent while Application and Infrastructure own validation and persistence.
/// </summary>
[ApiController]
[Authorize(Roles = "Reader,Admin")]
public sealed class ReaderPreferencesController : ControllerBase
{
    /// <summary>
    /// What: Returns the current signed-in reader's preferences.
    /// How: Calls GetReaderPreferences and returns the DTO as HTTP 200.
    /// Why: The frontend needs persisted theme, font, compact card, and preferred-category settings on startup.
    /// </summary>
    [HttpGet("/api/reader-preferences")]
    public IResult GetPreferences([FromServices] GetReaderPreferences useCase)
    {
        return Results.Ok(useCase.Execute(GetUserId()));
    }

    /// <summary>
    /// What: Replaces the current signed-in reader's preferences.
    /// How: ASP.NET binds the JSON body, Application validates it, and ApiResultMapping returns success or validation error.
    /// Why: A single replace endpoint keeps preference persistence predictable until real accounts exist.
    /// </summary>
    [HttpPut("/api/reader-preferences")]
    public IResult UpdatePreferences(
        [FromBody] UpdateReaderPreferencesRequest request,
        [FromServices] UpdateReaderPreferences useCase)
    {
        var result = useCase.Execute(GetUserId(), request);

        return ApiResultMapping.ToHttpResult(result);
    }

    /// <summary>
    /// What: Reads the authenticated user's stable id from the current request.
    /// How: the id is retrieved from the NameIdentifier claim that was written
    /// into the auth cookie during login or registration.
    /// Why: reader preferences are personal data and must be loaded or updated for
    /// exactly the signed-in account.
    /// </summary>
    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Authenticated request did not contain a user id claim.");
    }
}
