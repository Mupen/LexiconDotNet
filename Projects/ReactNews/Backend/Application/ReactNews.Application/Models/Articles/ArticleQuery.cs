namespace ReactNews.Application.Models.Articles;

/// <summary>
/// Validated article query passed from application use cases to news providers.
/// </summary>
/// <remarks>
/// What: contains normalized search/headline request values.
/// Why: this is an application/provider coordination model, not a domain entity.
/// The domain should describe business concepts like Article and Source; it
/// should not contain HTTP-style query criteria.
/// How: ArticleQueryFactory builds this model after applying defaults and
/// validation to raw API query-string values.
/// </remarks>
public sealed record ArticleQuery(
    string Mode,
    string? Query,
    string Country,
    string Category,
    string? Source,
    string Language,
    string SortBy,
    int Page,
    int PageSize);
