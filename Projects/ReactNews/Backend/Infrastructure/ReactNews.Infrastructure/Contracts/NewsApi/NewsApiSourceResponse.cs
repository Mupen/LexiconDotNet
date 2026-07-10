namespace ReactNews.Infrastructure.Contracts.NewsApi;

/// <summary>
/// What: Provider-specific response wrapper returned by the NewsAPI sources endpoint.
/// How: Contains status, source items, and optional error code/message fields from NewsAPI.
/// Why: The client needs the raw provider wrapper to detect provider errors before mapping valid source items.
/// </summary>
internal sealed record NewsApiSourceResponse(
    string Status,
    IReadOnlyList<NewsApiSourceItem> Sources,
    string? Code,
    string? Message);
