namespace ReactNews.Infrastructure.Options.NewsApi;

/// <summary>
/// Configuration values required by the NewsAPI infrastructure adapter.
/// </summary>
/// <remarks>
/// What: BaseUrl controls the external endpoint, ApiKey authenticates requests,
/// and CacheMinutes controls default cache duration.
/// Why: these values are infrastructure configuration, not application rules.
/// How: the API project binds appsettings/user-secrets into this options object
/// before Infrastructure creates the HttpClient.
/// </remarks>
public sealed class NewsApiOptions
{
    public const string SectionName = "NewsApi";

    public string BaseUrl { get; init; } = "https://newsapi.org/v2";

    public string ApiKey { get; init; } = string.Empty;

    public int CacheMinutes { get; init; } = 10;
}
