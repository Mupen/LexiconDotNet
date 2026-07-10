using System.Net;
using Microsoft.Extensions.Options;
using ReactNews.Application.Exceptions;
using ReactNews.Application.Models.Articles;
using ReactNews.Infrastructure.Options.NewsApi;
using ReactNews.Infrastructure.Providers.NewsApi;

namespace ReactNews.UnitTests.Infrastructure;

/// <summary>
/// What: Tests the NewsAPI HTTP client without making real network calls.
/// How: Uses a fake HttpMessageHandler to capture outgoing requests and return controlled JSON responses.
/// Why: The infrastructure client contains provider-specific URL, header, mapping, and error-handling rules that should be proven deterministically.
/// </summary>
public sealed class NewsApiProviderTests
{
    /// <summary>
    /// What: Checks that headline article requests use the correct NewsAPI endpoint and API-key header.
    /// How: Sends a headline query through the client and inspects the captured HttpRequestMessage.
    /// Why: A small path/header mistake would break the real integration even if the application layer is correct.
    /// </summary>
    [Fact]
    public async Task GetArticlesAsync_HeadlinesRequest_UsesTopHeadlinesEndpointAndApiKeyHeader()
    {
        var handler = new FakeHttpMessageHandler(ArticleJson());
        var provider = CreateProvider(handler);

        await provider.GetArticlesAsync(CreateHeadlineQuery(), CancellationToken.None);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("/v2/top-headlines", handler.LastRequest.RequestUri?.AbsolutePath);
        Assert.Contains("country=us", handler.LastRequest.RequestUri?.Query);
        Assert.Contains("category=technology", handler.LastRequest.RequestUri?.Query);
        Assert.Contains("page=2", handler.LastRequest.RequestUri?.Query);
        Assert.Contains("pageSize=20", handler.LastRequest.RequestUri?.Query);
        Assert.True(handler.LastRequest.Headers.TryGetValues("X-Api-Key", out var values));
        Assert.Equal("test-key", Assert.Single(values));
    }

    /// <summary>
    /// What: Checks that search article requests use NewsAPI's everything endpoint.
    /// How: Sends a search query and verifies the endpoint path plus query-string parameters.
    /// Why: Headlines and search are different provider endpoints, so tests make sure the client routes each mode correctly.
    /// </summary>
    [Fact]
    public async Task GetArticlesAsync_SearchRequest_UsesEverythingEndpoint()
    {
        var handler = new FakeHttpMessageHandler(ArticleJson());
        var provider = CreateProvider(handler);

        await provider.GetArticlesAsync(CreateSearchQuery(), CancellationToken.None);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("/v2/everything", handler.LastRequest.RequestUri?.AbsolutePath);
        Assert.Contains("q=dotnet", handler.LastRequest.RequestUri?.Query);
        Assert.Contains("language=en", handler.LastRequest.RequestUri?.Query);
        Assert.Contains("sortBy=publishedAt", handler.LastRequest.RequestUri?.Query);
    }

    /// <summary>
    /// What: Checks that valid provider articles are mapped and invalid provider articles are ignored.
    /// How: Returns JSON containing one valid article plus invalid articles missing required title/url values.
    /// Why: The domain model should not receive unusable articles, and filtering at the client boundary keeps later layers simpler.
    /// </summary>
    [Fact]
    public async Task GetArticlesAsync_MapsValidArticlesAndFiltersInvalidArticles()
    {
        var handler = new FakeHttpMessageHandler(ArticleJson());
        var provider = CreateProvider(handler);

        var result = await provider.GetArticlesAsync(CreateHeadlineQuery(), CancellationToken.None);

        Assert.Equal(3, result.TotalResults);
        Assert.False(result.FromCache);
        var article = Assert.Single(result.Articles);
        Assert.Equal("Example Source", article.SourceName);
        Assert.Equal("Jane Reporter", article.Author);
        Assert.Equal("Valid article", article.Title);
        Assert.Equal("https://example.com/news/valid", article.Url);
        Assert.NotEmpty(article.Id);
    }

    /// <summary>
    /// What: Checks source loading through the NewsAPI sources endpoint.
    /// How: Sends source filters, inspects the captured request, and verifies one mapped source comes back.
    /// Why: Source data powers filter UI and must use the provider-specific endpoint correctly.
    /// </summary>
    [Fact]
    public async Task GetSourcesAsync_UsesSourcesEndpointAndMapsSources()
    {
        var handler = new FakeHttpMessageHandler(SourceJson());
        var provider = CreateProvider(handler);

        var result = await provider.GetSourcesAsync("technology", "en", "us", CancellationToken.None);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("/v2/top-headlines/sources", handler.LastRequest.RequestUri?.AbsolutePath);
        Assert.Contains("category=technology", handler.LastRequest.RequestUri?.Query);
        Assert.Contains("language=en", handler.LastRequest.RequestUri?.Query);
        Assert.Contains("country=us", handler.LastRequest.RequestUri?.Query);
        var source = Assert.Single(result.Sources);
        Assert.Equal("example-source", source.Id);
        Assert.Equal("Example Source", source.Name);
        Assert.Equal("technology", source.Category);
    }

    /// <summary>
    /// What: Checks that a missing API key fails before any HTTP call is made.
    /// How: Creates the client with an empty key and asserts a NewsConfigurationException plus no captured request.
    /// Why: Configuration errors should be reported locally and clearly instead of sending a guaranteed-bad provider request.
    /// </summary>
    [Fact]
    public async Task GetArticlesAsync_ThrowsConfigurationException_WhenApiKeyIsMissing()
    {
        var handler = new FakeHttpMessageHandler(ArticleJson());
        var provider = CreateProvider(handler, apiKey: "");

        await Assert.ThrowsAsync<NewsConfigurationException>(() =>
            provider.GetArticlesAsync(CreateHeadlineQuery(), CancellationToken.None));

        Assert.Null(handler.LastRequest);
    }

    /// <summary>
    /// What: Checks that provider error responses become application-specific provider exceptions.
    /// How: Returns an Unauthorized response with NewsAPI-style error JSON and asserts the thrown exception message.
    /// Why: The rest of the backend should not need to understand NewsAPI's raw HTTP status and JSON error shape.
    /// </summary>
    [Fact]
    public async Task GetArticlesAsync_ThrowsProviderException_WhenNewsApiReturnsFailure()
    {
        var handler = new FakeHttpMessageHandler(
            """{"status":"error","code":"apiKeyInvalid","message":"Bad key"}""",
            HttpStatusCode.Unauthorized);
        var provider = CreateProvider(handler);

        var exception = await Assert.ThrowsAsync<NewsProviderException>(() =>
            provider.GetArticlesAsync(CreateHeadlineQuery(), CancellationToken.None));

        Assert.Equal("Bad key", exception.Message);
    }

    /// <summary>
    /// What: Creates a NewsApiClient configured for tests.
    /// How: Builds an HttpClient around the fake handler and passes test options through IOptions.
    /// Why: This keeps each test focused on behavior instead of repeating setup details.
    /// </summary>
    private static NewsApiClient CreateProvider(
        FakeHttpMessageHandler handler,
        string apiKey = "test-key")
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://newsapi.test/v2/")
        };

        var options = Options.Create(new NewsApiOptions
        {
            BaseUrl = "https://newsapi.test/v2",
            ApiKey = apiKey
        });

        return new NewsApiClient(httpClient, options);
    }

    /// <summary>
    /// What: Builds a representative headline query.
    /// How: Creates an ArticleQuery with headline mode, country/category filters, and page 2.
    /// Why: A stable helper makes request-URL assertions easier to understand and maintain.
    /// </summary>
    private static ArticleQuery CreateHeadlineQuery()
    {
        return new ArticleQuery(
            Mode: "headlines",
            Query: null,
            Country: "us",
            Category: "technology",
            Source: null,
            Language: "en",
            SortBy: "publishedAt",
            Page: 2,
            PageSize: 20);
    }

    /// <summary>
    /// What: Builds a representative search query.
    /// How: Creates an ArticleQuery with search mode and a q value of "dotnet".
    /// Why: Search requests use different provider parameters from headline requests, so a separate helper keeps the intent visible.
    /// </summary>
    private static ArticleQuery CreateSearchQuery()
    {
        return new ArticleQuery(
            Mode: "search",
            Query: "dotnet",
            Country: "us",
            Category: "general",
            Source: null,
            Language: "en",
            SortBy: "publishedAt",
            Page: 1,
            PageSize: 10);
    }

    /// <summary>
    /// What: Provides sample NewsAPI article JSON for client mapping tests.
    /// How: Returns a raw JSON string containing one valid article and two invalid articles.
    /// Why: Keeping the JSON close to the test makes the provider contract explicit without needing external fixture files.
    /// </summary>
    private static string ArticleJson()
    {
        return """
        {
          "status": "ok",
          "totalResults": 3,
          "articles": [
            {
              "source": { "id": "example-source", "name": "Example Source" },
              "author": "Jane Reporter",
              "title": "Valid article",
              "description": "Article description",
              "url": "https://example.com/news/valid",
              "urlToImage": "https://example.com/image.jpg",
              "publishedAt": "2026-07-09T10:00:00Z",
              "content": "Article content"
            },
            {
              "source": { "id": "bad-source", "name": "Bad Source" },
              "author": "No Url",
              "title": "Missing url",
              "description": "Should be filtered",
              "url": "",
              "urlToImage": null,
              "publishedAt": "2026-07-09T10:00:00Z",
              "content": null
            },
            {
              "source": { "id": "bad-source", "name": "Bad Source" },
              "author": "No Title",
              "title": "",
              "description": "Should be filtered",
              "url": "https://example.com/news/no-title",
              "urlToImage": null,
              "publishedAt": "2026-07-09T10:00:00Z",
              "content": null
            }
          ],
          "code": null,
          "message": null
        }
        """;
    }

    /// <summary>
    /// What: Provides sample NewsAPI source JSON for source mapping tests.
    /// How: Returns a raw JSON string containing one valid source and one invalid source.
    /// Why: The test should prove the client maps the provider response shape and filters incomplete records.
    /// </summary>
    private static string SourceJson()
    {
        return """
        {
          "status": "ok",
          "sources": [
            {
              "id": "example-source",
              "name": "Example Source",
              "description": "Source description",
              "url": "https://example.com",
              "category": "technology",
              "language": "en",
              "country": "us"
            },
            {
              "id": "",
              "name": "Invalid Source",
              "description": "Should be filtered",
              "url": "https://example.com/invalid",
              "category": "technology",
              "language": "en",
              "country": "us"
            }
          ],
          "code": null,
          "message": null
        }
        """;
    }

    /// <summary>
    /// What: Replaces the real HTTP transport for NewsApiClient tests.
    /// How: Captures the last outgoing request and returns a configured HttpResponseMessage.
    /// Why: This gives full control over provider responses while still testing the real HttpClient-based code path.
    /// </summary>
    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _responseJson;
        private readonly HttpStatusCode _statusCode;

        public FakeHttpMessageHandler(
            string responseJson,
            HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            // What: Stores the fake response body and status code.
            // How: Assigns constructor values to fields used by SendAsync.
            // Why: Individual tests need to simulate both successful and failing provider responses.
            _responseJson = responseJson;
            _statusCode = statusCode;
        }

        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // What: Handles the outgoing HttpClient request inside the test.
            // How: Saves the request for assertions and returns the configured JSON response immediately.
            // Why: This avoids internet access while still proving URL construction, headers, and response mapping.
            LastRequest = request;

            return Task.FromResult(new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_responseJson)
            });
        }
    }
}
