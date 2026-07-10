using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ReactNews.Api.Mapping.Common;
using ReactNews.Application.Queries.SavedArticles;

namespace ReactNews.Api.Controllers.SavedArticles;

/// <summary>
/// What: HTTP endpoints for the reader's saved articles.
/// How: Delegates listing, saving, and removing to Application use cases.
/// Why: Saved articles are a reader feature; the API should expose user intent without knowing EF Core table details.
/// </summary>
[ApiController]
[Authorize(Roles = "Reader,Admin")]
public sealed class SavedArticlesController : ControllerBase
{
    /// <summary>
    /// What: Lists the current reader's saved articles.
    /// How: Calls ListSavedArticles and returns its response as HTTP 200.
    /// Why: The frontend needs one route to render a saved-reading list.
    /// </summary>
    [HttpGet("/api/saved-articles")]
    public IResult GetSavedArticles([FromServices] ListSavedArticles useCase)
    {
        return Results.Ok(useCase.Execute(GetUserId()));
    }

    /// <summary>
    /// What: Saves one article by snapshot id.
    /// How: Calls SaveArticleForLater, which reads the snapshot store and writes the saved-article store.
    /// Why: The browser should save by the same article id it already uses for article detail routing.
    /// </summary>
    [HttpPost("/api/saved-articles/{articleId}")]
    public IResult SaveArticle(
        string articleId,
        [FromServices] SaveArticleForLater useCase)
    {
        var result = useCase.Execute(GetUserId(), articleId);

        return ApiResultMapping.ToHttpResult(result);
    }

    /// <summary>
    /// What: Removes one article from the saved list.
    /// How: Calls RemoveSavedArticle and maps success/not-found through ApiResultMapping.
    /// Why: DELETE should be the single API command for undoing a saved article.
    /// </summary>
    [HttpDelete("/api/saved-articles/{articleId}")]
    public IResult RemoveSavedArticle(
        string articleId,
        [FromServices] RemoveSavedArticle useCase)
    {
        var result = useCase.Execute(GetUserId(), articleId);

        return ApiResultMapping.ToHttpResult(result);
    }

    /// <summary>
    /// What: Reads the authenticated user's stable id from the current request.
    /// How: the id is stored as the NameIdentifier claim inside the authentication
    /// cookie created by AuthController.
    /// Why: saved articles must always be scoped to the signed-in user; throwing
    /// here surfaces a broken authentication pipeline instead of silently saving
    /// data without an owner.
    /// </summary>
    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Authenticated request did not contain a user id claim.");
    }
}
