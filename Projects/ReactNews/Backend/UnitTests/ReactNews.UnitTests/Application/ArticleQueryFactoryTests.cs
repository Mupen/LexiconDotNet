using ReactNews.Application.Services.Articles;

namespace ReactNews.UnitTests.Application;

/// <summary>
/// What: Verifies the query normalization rules used before the application asks the news client for data.
/// How: Calls <see cref="ArticleQueryFactory"/> directly with missing, messy, and invalid input values, then checks the normalized result.
/// Why: Query creation is a boundary between user/API input and the rest of the backend, so these tests protect the project from sending
/// inconsistent parameters deeper into the application or out to NewsAPI.
/// </summary>
public sealed class ArticleQueryFactoryTests
{
    /// <summary>
    /// What: Checks that an empty request becomes the default top-headlines request.
    /// How: Passes null values into the factory and asserts the fallback mode, country, category, language, sort, page, and page size.
    /// Why: Defaults keep the home page useful even when the frontend sends no filters, and they keep default behavior documented in one test.
    /// </summary>
    [Fact]
    public void Create_DefaultsToHeadlines_WhenValuesAreMissing()
    {
        var query = ArticleQueryFactory.Create(null, null, null, null, null, null, null, null, null);

        Assert.Equal("headlines", query.Mode);
        Assert.Null(query.Query);
        Assert.Equal("us", query.Country);
        Assert.Equal("general", query.Category);
        Assert.Equal("en", query.Language);
        Assert.Equal("publishedAt", query.SortBy);
        Assert.Equal(1, query.Page);
        Assert.Equal(20, query.PageSize);
    }

    /// <summary>
    /// What: Checks that user-provided search values are cleaned before being used by the application.
    /// How: Passes mixed casing and whitespace into the factory and asserts the trimmed/lowercased values in the created query.
    /// Why: Normalization prevents duplicated cache keys and avoids forcing every caller to remember the same string-cleaning rules.
    /// </summary>
    [Fact]
    public void Create_NormalizesSearchValues()
    {
        var query = ArticleQueryFactory.Create(
            " SEARCH ",
            "  dotnet  ",
            "SE",
            "Technology",
            "bbc-news",
            "SV",
            "popularity",
            2,
            50);

        Assert.Equal("search", query.Mode);
        Assert.Equal("dotnet", query.Query);
        Assert.Equal("se", query.Country);
        Assert.Equal("technology", query.Category);
        Assert.Equal("bbc-news", query.Source);
        Assert.Equal("sv", query.Language);
        Assert.Equal("popularity", query.SortBy);
        Assert.Equal(2, query.Page);
        Assert.Equal(50, query.PageSize);
    }

    /// <summary>
    /// What: Checks that invalid request shapes fail with clear validation messages.
    /// How: Uses theory data to test multiple invalid cases against the same factory method.
    /// Why: Validation errors are expected user/input failures, so the test makes sure they remain predictable and understandable.
    /// </summary>
    [Theory]
    [InlineData("bad-mode", "Mode must be either headlines or search.")]
    [InlineData("search", "Search query must be between 2 and 500 characters.")]
    public void Create_ThrowsValidationException_WhenRequestIsInvalid(string mode, string expectedMessage)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            ArticleQueryFactory.Create(mode, "x", "us", "general", null, "en", "publishedAt", 1, 20));

        Assert.Equal(expectedMessage, exception.Message);
    }

    /// <summary>
    /// What: Checks that paging values stay inside the allowed range.
    /// How: Sends an invalid negative page and an oversized page size, then verifies the clamped result.
    /// Why: Clamping protects the external API and database/cache layers from unreasonable request sizes while still accepting imperfect input.
    /// </summary>
    [Fact]
    public void Create_ClampsPageAndPageSize()
    {
        var query = ArticleQueryFactory.Create("headlines", null, "us", "general", null, "en", null, -10, 500);

        Assert.Equal(1, query.Page);
        Assert.Equal(100, query.PageSize);
    }

    /// <summary>
    /// What: Checks that optional filters can be intentionally absent.
    /// How: Passes null, empty, and whitespace values into the optional normalizers.
    /// Why: Returning null for empty optional filters is clearer than passing empty strings through the application and handling them everywhere.
    /// </summary>
    [Fact]
    public void NormalizeOptionalValues_ReturnsNull_WhenInputsAreEmpty()
    {
        Assert.Null(ArticleQueryFactory.NormalizeOptionalCategory(""));
        Assert.Null(ArticleQueryFactory.NormalizeOptionalLanguage(null));
        Assert.Null(ArticleQueryFactory.NormalizeOptionalCountry(" "));
    }
}
