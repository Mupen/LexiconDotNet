using System.Globalization;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using ReactNews.Application.Exceptions;
using ReactNews.Application.Interfaces;
using ReactNews.Application.Models.Articles;
using ReactNews.Application.Models.Sources;
using ReactNews.Domain.Entities.Articles;
using ReactNews.Domain.Entities.Sources;
using ReactNews.Infrastructure.Contracts.NewsApi;
using ReactNews.Infrastructure.Options.NewsApi;
using ReactNews.Infrastructure.Providers;

namespace ReactNews.Infrastructure.Providers.NewsApi;

/// <summary>
/// NewsAPI.org implementation of the INewsProvider application interface.
/// </summary>
/// <remarks>
/// What: this class builds NewsAPI HTTP requests, reads NewsAPI JSON responses,
/// maps them into domain records.
/// Why: all of that is infrastructure because it depends on a specific external
/// service, HTTP, API-key authentication, and provider response contracts.
/// How: use cases pass a validated ArticleQuery. This provider converts it to a
/// NewsAPI route, sends the request with X-Api-Key, maps the response, and
/// returns provider results to the application layer. Caching is handled by a
/// separate decorator so provider I/O and cache policy do not mix.
/// </remarks>
public sealed class NewsApiClient : INewsProviderSource
{
    private readonly HttpClient _httpClient;
    private readonly NewsApiOptions _options;

    public NewsApiClient(
        HttpClient httpClient,
        IOptions<NewsApiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    /// <summary>
    /// Gets article results from the appropriate NewsAPI article endpoint.
    /// </summary>
    /// <remarks>
    /// What: executes either a headline request or a search request.
    /// How: the validated ArticleQuery decides whether BuildHeadlinePath or
    /// BuildSearchPath is used, then SendArticleRequestAsync performs the HTTP
    /// call and maps the JSON response.
    /// Why: NewsAPI splits headlines and search into different endpoints, but the
    /// Application layer should work with one article-provider method.
    /// </remarks>
    public async Task<ArticleProviderResult> GetArticlesAsync(
        ArticleQuery query,
        CancellationToken cancellationToken)
    {
        EnsureApiKeyExists();

        var cachedUntilUtc = GetCacheExpiration(query.Mode);
        var path = query.Mode == "search"
            ? BuildSearchPath(query)
            : BuildHeadlinePath(query);

        return await SendArticleRequestAsync(path, cachedUntilUtc, cancellationToken);
    }

    /// <summary>
    /// Gets source metadata from NewsAPI.
    /// </summary>
    /// <remarks>
    /// What: calls the NewsAPI top-headlines/sources endpoint.
    /// How: optional category, language, and country filters are added only when
    /// supplied, then SendSourceRequestAsync handles HTTP and mapping.
    /// Why: source metadata supports filters in the UI and future source
    /// management without exposing the frontend to NewsAPI directly.
    /// </remarks>
    public async Task<SourceProviderResult> GetSourcesAsync(
        string? category,
        string? language,
        string? country,
        CancellationToken cancellationToken)
    {
        EnsureApiKeyExists();

        var parameters = new Dictionary<string, string>();

        if (category is not null)
        {
            parameters["category"] = category;
        }

        if (language is not null)
        {
            parameters["language"] = language;
        }

        if (country is not null)
        {
            parameters["country"] = country;
        }

        var cachedUntilUtc = DateTimeOffset.UtcNow.AddMinutes(15);
        return await SendSourceRequestAsync(
            BuildPath("top-headlines/sources", parameters),
            cachedUntilUtc,
            cancellationToken);
    }

    /// <summary>
    /// Sends a NewsAPI article request and maps the result.
    /// </summary>
    /// <remarks>
    /// What: performs the HTTP call for /top-headlines or /everything.
    /// How: the API key is sent in the X-Api-Key header, JSON is deserialized
    /// into provider-specific contracts, invalid provider responses throw
    /// NewsProviderException, and valid articles are filtered/mapped.
    /// Why: this keeps all external-provider behavior in Infrastructure so the
    /// Application layer receives clean ArticleProviderResult objects.
    /// </remarks>
    private async Task<ArticleProviderResult> SendArticleRequestAsync(
        string path,
        DateTimeOffset cachedUntilUtc,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Add("X-Api-Key", _options.ApiKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var newsResponse = await response.Content.ReadFromJsonAsync<NewsApiArticleResponse>(cancellationToken);

        if (!response.IsSuccessStatusCode || newsResponse is null || !string.Equals(newsResponse.Status, "ok", StringComparison.OrdinalIgnoreCase))
        {
            var message = newsResponse?.Message ?? response.ReasonPhrase ?? "News API request failed.";
            throw new NewsProviderException(message);
        }

        var articles = newsResponse.Articles
            .Where(article => !string.IsNullOrWhiteSpace(article.Title) && !string.IsNullOrWhiteSpace(article.Url))
            .Select(MapArticle)
            .ToList();

        return new ArticleProviderResult(
            Articles: articles,
            TotalResults: newsResponse.TotalResults,
            FetchedAtUtc: DateTimeOffset.UtcNow,
            CachedUntilUtc: cachedUntilUtc,
            FromCache: false);
    }

    /// <summary>
    /// Sends a NewsAPI source request and maps the result.
    /// </summary>
    /// <remarks>
    /// What: performs the HTTP call for /top-headlines/sources.
    /// How: the API key is sent in the X-Api-Key header, JSON is deserialized
    /// into NewsApiSourceResponse, failed provider responses throw, and valid
    /// source records are mapped into domain Source values.
    /// Why: source mapping rules belong beside the provider contracts because
    /// they are specific to NewsAPI's response shape.
    /// </remarks>
    private async Task<SourceProviderResult> SendSourceRequestAsync(
        string path,
        DateTimeOffset cachedUntilUtc,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Add("X-Api-Key", _options.ApiKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var sourceResponse = await response.Content.ReadFromJsonAsync<NewsApiSourceResponse>(cancellationToken);

        if (!response.IsSuccessStatusCode || sourceResponse is null || !string.Equals(sourceResponse.Status, "ok", StringComparison.OrdinalIgnoreCase))
        {
            var message = sourceResponse?.Message ?? response.ReasonPhrase ?? "News API source request failed.";
            throw new NewsProviderException(message);
        }

        var sources = sourceResponse.Sources
            .Where(source => !string.IsNullOrWhiteSpace(source.Id) && !string.IsNullOrWhiteSpace(source.Name))
            .Select(source => new Source(
                Id: source.Id?.Trim() ?? string.Empty,
                Name: source.Name?.Trim() ?? string.Empty,
                Description: EmptyToNull(source.Description),
                Url: EmptyToNull(source.Url),
                Category: EmptyToNull(source.Category),
                Language: EmptyToNull(source.Language),
                Country: EmptyToNull(source.Country)))
            .ToList();

        return new SourceProviderResult(
            Sources: sources,
            FetchedAtUtc: DateTimeOffset.UtcNow,
            CachedUntilUtc: cachedUntilUtc,
            FromCache: false);
    }

    /// <summary>
    /// Maps one NewsAPI article contract into a domain Article.
    /// </summary>
    /// <remarks>
    /// What: translates external field names and null behavior into the stable
    /// Article record used by the rest of ReactNews.
    /// How: important strings are trimmed, blank optional values become null, and
    /// a deterministic id is generated from URL/title/source.
    /// Why: the frontend and application should not depend on NewsAPI's raw JSON
    /// shape or missing-value quirks.
    /// </remarks>
    private static Article MapArticle(NewsApiArticle article)
    {
        var title = article.Title?.Trim() ?? "Untitled article";
        var url = article.Url?.Trim() ?? string.Empty;
        var sourceName = article.Source?.Name?.Trim();

        return new Article(
            Id: CreateArticleId(url, title, sourceName),
            SourceName: sourceName,
            Author: EmptyToNull(article.Author),
            Title: title,
            Description: EmptyToNull(article.Description),
            Url: url,
            ImageUrl: EmptyToNull(article.UrlToImage),
            PublishedAt: article.PublishedAt,
            Content: EmptyToNull(article.Content));
    }

    /// <summary>
    /// Builds the relative path for a NewsAPI top-headlines request.
    /// </summary>
    /// <remarks>
    /// What: creates the endpoint and query string for headline mode.
    /// How: country/category/page/pageSize are used by default; if a source is
    /// supplied, NewsAPI requires sources instead of country/category, so those
    /// parameters are removed.
    /// Why: centralizing this rule prevents request-building mistakes and makes
    /// the NewsAPI-specific source behavior visible.
    /// </remarks>
    private static string BuildHeadlinePath(ArticleQuery query)
    {
        var parameters = new Dictionary<string, string>
        {
            ["country"] = query.Country,
            ["category"] = query.Category,
            ["page"] = query.Page.ToString(CultureInfo.InvariantCulture),
            ["pageSize"] = query.PageSize.ToString(CultureInfo.InvariantCulture)
        };

        if (query.Source is not null)
        {
            parameters.Remove("country");
            parameters.Remove("category");
            parameters["sources"] = query.Source;
        }

        return BuildPath("top-headlines", parameters);
    }

    /// <summary>
    /// Builds the relative path for a NewsAPI everything/search request.
    /// </summary>
    /// <remarks>
    /// What: creates the endpoint and query string for search mode.
    /// How: query text, language, sort, page, pageSize, and optional source are
    /// converted into query parameters.
    /// Why: search requests have different parameters from headline requests, so
    /// a separate method keeps both paths easier to verify and test.
    /// </remarks>
    private static string BuildSearchPath(ArticleQuery query)
    {
        var parameters = new Dictionary<string, string>
        {
            ["q"] = query.Query ?? string.Empty,
            ["language"] = query.Language,
            ["sortBy"] = query.SortBy,
            ["page"] = query.Page.ToString(CultureInfo.InvariantCulture),
            ["pageSize"] = query.PageSize.ToString(CultureInfo.InvariantCulture)
        };

        if (query.Source is not null)
        {
            parameters["sources"] = query.Source;
        }

        return BuildPath("everything", parameters);
    }

    /// <summary>
    /// Builds a URL path with encoded query parameters.
    /// </summary>
    /// <remarks>
    /// What: combines an endpoint name and a parameter dictionary.
    /// How: Uri.EscapeDataString encodes both keys and values before joining them
    /// with ampersands.
    /// Why: manually concatenating raw user/search text into URLs can break when
    /// values contain spaces, symbols, or non-English characters.
    /// </remarks>
    private static string BuildPath(string endpoint, IReadOnlyDictionary<string, string> parameters)
    {
        var query = string.Join("&", parameters.Select(item =>
            $"{Uri.EscapeDataString(item.Key)}={Uri.EscapeDataString(item.Value)}"));

        return query.Length == 0 ? endpoint : $"{endpoint}?{query}";
    }

    /// <summary>
    /// Creates ReactNews' generated article id.
    /// </summary>
    /// <remarks>
    /// What: produces a stable short id for an article snapshot.
    /// How: URL, title, and source are joined, hashed with SHA-256, then shortened
    /// to the first 16 hex characters.
    /// Why: NewsAPI does not give this app a durable article id. A deterministic
    /// id lets the frontend route to /article/{id} and lets the backend store
    /// snapshots by key.
    /// </remarks>
    private static string CreateArticleId(string url, string title, string? sourceName)
    {
        var input = $"{url}|{title}|{sourceName}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash)[..16].ToLowerInvariant();
    }

    /// <summary>
    /// Chooses the provider result expiry time.
    /// </summary>
    /// <remarks>
    /// What: returns a DateTimeOffset used by cache and snapshot storage.
    /// How: search results get five minutes, while headline results get two
    /// minutes.
    /// Why: headlines are expected to change faster than general searches, and
    /// shorter headline caching keeps the app closer to current news.
    /// </remarks>
    private static DateTimeOffset GetCacheExpiration(string mode)
    {
        return DateTimeOffset.UtcNow.Add(mode == "search" ? TimeSpan.FromMinutes(5) : TimeSpan.FromMinutes(2));
    }

    /// <summary>
    /// Converts blank provider strings into null.
    /// </summary>
    /// <remarks>
    /// What: normalizes optional string fields from NewsAPI.
    /// How: null, empty, or whitespace returns null; nonblank values are trimmed.
    /// Why: the rest of the app can treat null consistently as "not provided"
    /// instead of checking many blank-string variants.
    /// </remarks>
    private static string? EmptyToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    /// <summary>
    /// Ensures the NewsAPI key is configured before making HTTP calls.
    /// </summary>
    /// <remarks>
    /// What: throws a configuration exception when ApiKey is missing.
    /// How: string.IsNullOrWhiteSpace catches null, empty, and whitespace values.
    /// Why: failing before the HTTP request gives a clearer local setup error
    /// than sending an unauthenticated request and interpreting NewsAPI's error.
    /// </remarks>
    private void EnsureApiKeyExists()
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new NewsConfigurationException("NewsApi:ApiKey is missing. Set it with user-secrets locally or NEWSAPI_KEY/.env when using Docker.");
        }
    }

}
