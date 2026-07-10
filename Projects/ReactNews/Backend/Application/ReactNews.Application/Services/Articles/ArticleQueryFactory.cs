using ReactNews.Application.Models.Articles;

namespace ReactNews.Application.Services.Articles;

/// <summary>
/// Converts raw API query values into a validated ArticleQuery.
/// </summary>
/// <remarks>
/// What: this class owns defaults and validation for article browsing/searching.
/// Why: validation belongs in the application layer because it is part of the
/// use case contract. Infrastructure should receive clean input, and controllers
/// should not duplicate rules that other entry points might also need.
/// How: each Normalize method handles one concept. The public Create method then
/// combines those normalized values into the domain query object used downstream.
/// </remarks>
public static class ArticleQueryFactory
{
    private static readonly HashSet<string> AllowedCategories = new(StringComparer.OrdinalIgnoreCase)
    {
        "business",
        "entertainment",
        "general",
        "health",
        "science",
        "sports",
        "technology"
    };

    private static readonly HashSet<string> AllowedSorts = new(StringComparer.OrdinalIgnoreCase)
    {
        "publishedAt",
        "popularity",
        "relevancy"
    };

    /// <summary>
    /// Builds one validated article query from raw request values.
    /// </summary>
    /// <remarks>
    /// What: converts nullable strings and nullable numbers from controllers or
    /// use cases into a complete ArticleQuery.
    /// How: mode is normalized first, then search and headline modes apply their
    /// own defaults while shared values such as country/page/pageSize use helper
    /// normalization methods.
    /// Why: infrastructure should receive valid, predictable query values. Putting
    /// these rules here prevents controllers and providers from duplicating or
    /// disagreeing about defaults.
    /// </remarks>
    public static ArticleQuery Create(
        string? mode,
        string? query,
        string? country,
        string? category,
        string? source,
        string? language,
        string? sortBy,
        int? page,
        int? pageSize)
    {
        var normalizedMode = string.IsNullOrWhiteSpace(mode)
            ? "headlines"
            : mode.Trim().ToLowerInvariant();

        if (normalizedMode is not "headlines" and not "search")
        {
            throw new ArgumentException("Mode must be either headlines or search.");
        }

        if (normalizedMode == "search")
        {
            return new ArticleQuery(
                Mode: normalizedMode,
                Query: NormalizeSearchQuery(query ?? string.Empty),
                Country: NormalizeCountry(country),
                Category: NormalizeCategory(category),
                Source: EmptyToNull(source),
                Language: NormalizeLanguage(language),
                SortBy: NormalizeSort(sortBy),
                Page: NormalizePage(page),
                PageSize: NormalizePageSize(pageSize));
        }

        return new ArticleQuery(
            Mode: normalizedMode,
            Query: null,
            Country: NormalizeCountry(country),
            Category: NormalizeCategory(category),
            Source: EmptyToNull(source),
            Language: NormalizeLanguage(language),
            SortBy: "publishedAt",
            Page: NormalizePage(page),
            PageSize: NormalizePageSize(pageSize));
    }

    /// <summary>
    /// Normalizes an optional category filter.
    /// </summary>
    /// <remarks>
    /// What: returns null when no category is supplied, otherwise validates the
    /// supplied category.
    /// How: blank values are treated as absent; nonblank values go through
    /// NormalizeCategory.
    /// Why: source-list filters are optional, while article-list categories have a
    /// default. This helper allows both workflows to share the same validation.
    /// </remarks>
    public static string? NormalizeOptionalCategory(string? category)
    {
        return string.IsNullOrWhiteSpace(category) ? null : NormalizeCategory(category);
    }

    /// <summary>
    /// Normalizes an optional language filter.
    /// </summary>
    /// <remarks>
    /// What: returns null for missing language and a validated two-letter code for
    /// supplied language.
    /// How: blank values are treated as absent; nonblank values go through
    /// NormalizeLanguage.
    /// Why: NewsAPI source requests can omit language, but invalid language codes
    /// should still be rejected before reaching infrastructure.
    /// </remarks>
    public static string? NormalizeOptionalLanguage(string? language)
    {
        return string.IsNullOrWhiteSpace(language) ? null : NormalizeLanguage(language);
    }

    /// <summary>
    /// Normalizes an optional country filter.
    /// </summary>
    /// <remarks>
    /// What: returns null for missing country and a validated two-letter code for
    /// supplied country.
    /// How: blank values are treated as absent; nonblank values go through
    /// NormalizeCountry.
    /// Why: source requests can choose not to filter by country, while malformed
    /// country codes should still be caught consistently.
    /// </remarks>
    public static string? NormalizeOptionalCountry(string? country)
    {
        return string.IsNullOrWhiteSpace(country) ? null : NormalizeCountry(country);
    }

    /// <summary>
    /// Converts a country value into a lower-case two-letter country code.
    /// </summary>
    /// <remarks>
    /// What: applies the default country and validates country format.
    /// How: missing values become us; supplied values are trimmed, lower-cased,
    /// checked for length 2, and checked for ASCII letters.
    /// Why: NewsAPI expects short country codes. Validating early gives a clearer
    /// application error than sending bad values to the external provider.
    /// </remarks>
    private static string NormalizeCountry(string? country)
    {
        var normalized = string.IsNullOrWhiteSpace(country)
            ? "us"
            : country.Trim().ToLowerInvariant();

        if (normalized.Length != 2 || !normalized.All(char.IsAsciiLetter))
        {
            throw new ArgumentException("Country must be a 2-letter ISO code, for example us, se, or gb.");
        }

        return normalized;
    }

    /// <summary>
    /// Converts a category value into a supported NewsAPI category.
    /// </summary>
    /// <remarks>
    /// What: applies the default category and rejects unsupported values.
    /// How: missing values become general; supplied values are trimmed,
    /// lower-cased, and checked against AllowedCategories.
    /// Why: accepting arbitrary category strings would produce provider errors and
    /// make the frontend harder to reason about.
    /// </remarks>
    private static string NormalizeCategory(string? category)
    {
        var normalized = string.IsNullOrWhiteSpace(category)
            ? "general"
            : category.Trim().ToLowerInvariant();

        if (!AllowedCategories.Contains(normalized))
        {
            throw new ArgumentException("Category must be one of: business, entertainment, general, health, science, sports, technology.");
        }

        return normalized;
    }

    /// <summary>
    /// Validates the free-text search query.
    /// </summary>
    /// <remarks>
    /// What: trims search text and enforces a reasonable length range.
    /// How: values shorter than 2 or longer than 500 characters throw an
    /// ArgumentException.
    /// Why: empty/one-character searches are usually accidental and very long
    /// searches are not useful for this teaching app or NewsAPI quota usage.
    /// </remarks>
    private static string NormalizeSearchQuery(string query)
    {
        var normalized = query.Trim();

        if (normalized.Length is < 2 or > 500)
        {
            throw new ArgumentException("Search query must be between 2 and 500 characters.");
        }

        return normalized;
    }

    /// <summary>
    /// Converts a language value into a lower-case two-letter language code.
    /// </summary>
    /// <remarks>
    /// What: applies the default language and validates language format.
    /// How: missing values become en; supplied values are trimmed, lower-cased,
    /// checked for length 2, and checked for ASCII letters.
    /// Why: this mirrors the country-code guard and keeps invalid external API
    /// requests out of infrastructure.
    /// </remarks>
    private static string NormalizeLanguage(string? language)
    {
        var normalized = string.IsNullOrWhiteSpace(language)
            ? "en"
            : language.Trim().ToLowerInvariant();

        if (normalized.Length != 2 || !normalized.All(char.IsAsciiLetter))
        {
            throw new ArgumentException("Language must be a 2-letter ISO code, for example en, sv, or de.");
        }

        return normalized;
    }

    /// <summary>
    /// Converts a sort value into a supported NewsAPI search sort.
    /// </summary>
    /// <remarks>
    /// What: applies the default sort and rejects unsupported sort values.
    /// How: blank values become publishedAt; supplied values are checked against
    /// AllowedSorts.
    /// Why: sortBy only applies to NewsAPI search mode. Validating here keeps
    /// provider code focused on HTTP rather than use-case validation.
    /// </remarks>
    private static string NormalizeSort(string? sortBy)
    {
        var normalized = string.IsNullOrWhiteSpace(sortBy)
            ? "publishedAt"
            : sortBy.Trim();

        if (!AllowedSorts.Contains(normalized))
        {
            throw new ArgumentException("sortBy must be one of: publishedAt, popularity, relevancy.");
        }

        return normalized;
    }

    /// <summary>
    /// Normalizes the requested page number.
    /// </summary>
    /// <remarks>
    /// What: converts missing or extreme page values into the supported range.
    /// How: missing values become 1 and Math.Clamp limits the result to 1-100.
    /// Why: this protects the backend from bad URLs and mirrors NewsAPI's
    /// practical paging limits.
    /// </remarks>
    private static int NormalizePage(int? page)
    {
        return Math.Clamp(page ?? 1, 1, 100);
    }

    /// <summary>
    /// Normalizes the requested page size.
    /// </summary>
    /// <remarks>
    /// What: converts missing or extreme page-size values into the supported
    /// range.
    /// How: missing values become 20 and Math.Clamp limits the result to 1-100.
    /// Why: a bounded page size protects API quota, response size, and frontend
    /// rendering performance.
    /// </remarks>
    private static int NormalizePageSize(int? pageSize)
    {
        return Math.Clamp(pageSize ?? 20, 1, 100);
    }

    /// <summary>
    /// Converts blank strings into null.
    /// </summary>
    /// <remarks>
    /// What: normalizes optional string values.
    /// How: null, empty, or whitespace values return null; real values are
    /// trimmed.
    /// Why: null is easier for downstream code to interpret as "not supplied"
    /// than many different blank-string variations.
    /// </remarks>
    private static string? EmptyToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
