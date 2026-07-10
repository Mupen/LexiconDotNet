using ReactNews.Application.Interfaces;
using ReactNews.Application.Mapping;
using ReactNews.Application.Contracts.Common;
using ReactNews.Application.Contracts.Sources;
using ReactNews.Application.Services.Articles;

namespace ReactNews.Application.Queries.Sources;

/// <summary>
/// Gets source metadata used by source filters in the frontend.
/// </summary>
/// <remarks>
/// What: loads source metadata from the configured news feed client.
/// How: optional filters are normalized before calling INewsProvider.
/// Why: source validation belongs in Application so controllers and
/// infrastructure stay thin and focused on their own responsibilities.
/// </remarks>
public sealed class GetSources
{
    private readonly INewsProvider _newsProvider;

    public GetSources(INewsProvider newsProvider)
    {
        _newsProvider = newsProvider;
    }

    /// <summary>
    /// Executes the source-list use case.
    /// </summary>
    /// <remarks>
    /// What: validates optional source filters and returns source DTOs.
    /// How: ArticleQueryFactory normalizes optional category/language/country
    /// values; INewsProvider loads source data; NewsMapping creates DTOs.
    /// Why: invalid filters are expected user/request errors, so they return a
    /// Result failure instead of throwing through the API pipeline.
    /// </remarks>
    public async Task<Result<SourceListResponse>> ExecuteAsync(
        SourceListRequest request,
        CancellationToken cancellationToken)
    {
        string? category;
        string? language;
        string? country;

        try
        {
            category = ArticleQueryFactory.NormalizeOptionalCategory(request.Category);
            language = ArticleQueryFactory.NormalizeOptionalLanguage(request.Language);
            country = ArticleQueryFactory.NormalizeOptionalCountry(request.Country);
        }
        catch (ArgumentException ex)
        {
            return Result<SourceListResponse>.Failure(Error.Validation(ex.Message));
        }

        var providerResult = await _newsProvider.GetSourcesAsync(category, language, country, cancellationToken);

        return Result<SourceListResponse>.Success(new SourceListResponse(
            Items: providerResult.Sources.Select(source => source.ToDto()).ToList(),
            FromCache: providerResult.FromCache,
            FetchedAtUtc: providerResult.FetchedAtUtc,
            CachedUntilUtc: providerResult.CachedUntilUtc));
    }
}
