using ReactNews.Application.Contracts.Sources;
using ReactNews.Application.Interfaces;
using ReactNews.Application.Models.Articles;
using ReactNews.Application.Models.Sources;
using ReactNews.Application.Queries.Sources;
using ReactNews.Domain.Entities.Articles;
using ReactNews.Domain.Entities.Sources;

namespace ReactNews.UnitTests.Application;

/// <summary>
/// What: Tests the source-list use case without depending on the real NewsAPI service.
/// How: Uses an in-memory fake news provider that records the inputs and returns predefined source data.
/// Why: Use-case tests should focus on application behavior, validation, and mapping, not on network availability or provider-specific behavior.
/// </summary>
public sealed class SourceUseCaseTests
{
    /// <summary>
    /// What: Checks that invalid source filter values are rejected.
    /// How: Calls the use case with an unsupported category and asserts a validation failure result.
    /// Why: Invalid filters should stop at the application layer so the infrastructure client does not receive impossible provider requests.
    /// </summary>
    [Fact]
    public async Task GetSources_ReturnsValidationFailure_WhenCategoryIsInvalid()
    {
        var useCase = new GetSources(new FakeNewsProvider());

        var result = await useCase.ExecuteAsync(
            new SourceListRequest("invalid", "en", "us"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("validation_error", result.Error?.Code);
    }

    /// <summary>
    /// What: Checks that provider source entities are returned as application DTOs.
    /// How: Seeds the fake provider with one source, runs the use case, and verifies both the mapped DTO and the normalized provider inputs.
    /// Why: This protects the contract between the application layer and the API layer while still confirming the provider receives clean filters.
    /// </summary>
    [Fact]
    public async Task GetSources_ReturnsMappedSources()
    {
        var provider = new FakeNewsProvider();
        provider.Sources.Add(new Source(
            Id: "bbc-news",
            Name: "BBC News",
            Description: "News source",
            Url: "https://bbc.com",
            Category: "general",
            Language: "en",
            Country: "gb"));
        var useCase = new GetSources(provider);

        var result = await useCase.ExecuteAsync(
            new SourceListRequest("general", "en", "gb"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value.Items);
        Assert.Equal("bbc-news", result.Value.Items[0].Id);
        Assert.Equal("general", provider.LastCategory);
        Assert.Equal("en", provider.LastLanguage);
        Assert.Equal("gb", provider.LastCountry);
    }

    /// <summary>
    /// What: Provides a controlled in-memory replacement for the real news provider.
    /// How: Stores source data in a list, records the last filter values, and returns provider result objects from Task.FromResult.
    /// Why: A fake keeps these tests fast, deterministic, and focused on the use case instead of the HTTP client or external API.
    /// </summary>
    private sealed class FakeNewsProvider : INewsProvider
    {
        public List<Source> Sources { get; } = new();

        public string? LastCategory { get; private set; }

        public string? LastLanguage { get; private set; }

        public string? LastCountry { get; private set; }

        public Task<ArticleProviderResult> GetArticlesAsync(
            ArticleQuery query,
            CancellationToken cancellationToken)
        {
            // What: Satisfies the INewsProvider interface for tests that do not need articles.
            // How: Returns an empty article result immediately.
            // Why: The source tests only exercise GetSourcesAsync, but the fake must still implement the full interface contract.
            return Task.FromResult(new ArticleProviderResult(
                Array.Empty<Article>(),
                0,
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow.AddMinutes(5),
                FromCache: false));
        }

        /// <summary>
        /// What: Captures source filters and returns seeded source data.
        /// How: stores the received arguments in public properties, then returns
        /// the in-memory source list as a provider result.
        /// Why: tests need to verify both response mapping and the normalized
        /// filters that would be sent to the real provider.
        /// </summary>
        public Task<SourceProviderResult> GetSourcesAsync(
            string? category,
            string? language,
            string? country,
            CancellationToken cancellationToken)
        {
            // What: Captures the filters passed by the use case and returns the seeded source list.
            // How: Copies the method arguments into properties before constructing a SourceProviderResult.
            // Why: The test needs to verify both output mapping and the values that would have been sent to the real provider.
            LastCategory = category;
            LastLanguage = language;
            LastCountry = country;

            return Task.FromResult(new SourceProviderResult(
                Sources,
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow.AddMinutes(5),
                FromCache: false));
        }
    }
}
