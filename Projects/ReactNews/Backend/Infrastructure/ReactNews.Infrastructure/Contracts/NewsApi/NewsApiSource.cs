namespace ReactNews.Infrastructure.Contracts.NewsApi;

/// <summary>
/// What: Nested provider-specific source object inside a NewsAPI article.
/// How: Stores only the id/name fields NewsAPI includes on article responses.
/// Why: Article responses have a smaller source shape than the full source endpoint, so a separate contract keeps that difference explicit.
/// </summary>
internal sealed record NewsApiSource(string? Id, string? Name);
