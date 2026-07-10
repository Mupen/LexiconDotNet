using Microsoft.AspNetCore.Mvc;
using ReactNews.Api.Mapping.Common;
using ReactNews.Application.Contracts.Sources;
using ReactNews.Application.Exceptions;
using ReactNews.Application.Queries.Sources;

namespace ReactNews.Api.Controllers.Sources;

/// <summary>
/// HTTP endpoints for NewsAPI source metadata.
/// </summary>
/// <remarks>
/// What: exposes source metadata to frontend or future filter controls.
/// How: query-string filters are wrapped in SourceListRequest and passed to the
/// Application use case.
/// Why: source lookup should go through the backend so the browser never needs a
/// NewsAPI key.
/// </remarks>
[ApiController]
public sealed class SourcesController : ControllerBase
{
    /// <summary>
    /// Gets filtered NewsAPI source metadata.
    /// </summary>
    /// <remarks>
    /// What: accepts optional category/language/country filters.
    /// How: GetSources validates optional filters and calls the news feed client.
    /// Why: source filters are useful UI data, but invalid values should still be
    /// handled consistently through application validation.
    /// </remarks>
    [HttpGet("/api/sources")]
    public async Task<IResult> GetSources(
        [FromQuery] string? category,
        [FromQuery] string? language,
        [FromQuery] string? country,
        [FromServices] GetSources useCase,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await useCase.ExecuteAsync(
                new SourceListRequest(category, language, country),
                cancellationToken);

            return ApiResultMapping.ToHttpResult(result);
        }
        catch (Exception ex) when (ex is NewsConfigurationException or NewsProviderException)
        {
            return ApiExceptionMapping.ToProblemResult(ex);
        }
    }
}
