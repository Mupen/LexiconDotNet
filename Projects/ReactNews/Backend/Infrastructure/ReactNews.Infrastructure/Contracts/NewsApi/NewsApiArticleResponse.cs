namespace ReactNews.Infrastructure.Contracts.NewsApi;

/// <summary>
/// Response contract returned by NewsAPI article endpoints.
/// </summary>
/// <remarks>
/// What: mirrors the JSON shape from /top-headlines and /everything.
/// Why: this type belongs in Infrastructure because it is coupled to one
/// external provider. Application and Domain should never depend on it.
/// How: NewsApiClient deserializes HTTP JSON into this record and maps it
/// into domain Article entities.
/// </remarks>
internal sealed record NewsApiArticleResponse(
    string Status,
    int TotalResults,
    IReadOnlyList<NewsApiArticle> Articles,
    string? Code,
    string? Message);
