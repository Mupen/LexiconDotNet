using System.Text.RegularExpressions;
using ReactNews.Application.Contracts.EditorialArticles;
using ReactNews.Domain.Entities.EditorialArticles;
using ReactNews.Domain.Enums.EditorialArticles;

namespace ReactNews.Application.Services.EditorialArticles;

/// <summary>
/// What: Creates validated editorial article domain objects from API requests.
/// How: Normalizes text fields, generates slugs, validates status/category, and applies timestamps.
/// Why: Editorial validation is application/business behavior and should not live in controllers or EF records.
/// </summary>
public static partial class EditorialArticleFactory
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

    public static EditorialArticle Create(EditorialArticleRequest request, DateTimeOffset nowUtc)
    {
        var title = RequireText(request.Title, "Title", 5, 180);
        var summary = RequireText(request.Summary, "Summary", 10, 500);
        var body = RequireText(request.Body, "Body", 50, 20000);
        var author = RequireText(request.Author, "Author", 2, 120);
        var category = NormalizeCategory(request.Category);
        var status = NormalizeStatus(request.Status);
        DateTimeOffset? publishedAt = status == EditorialArticleStatus.Published ? nowUtc : null;

        return new EditorialArticle(
            Id: Guid.NewGuid().ToString("N"),
            Title: title,
            Slug: CreateSlug(title),
            Summary: summary,
            Body: body,
            Author: author,
            Category: category,
            ImageUrl: EmptyToNull(request.ImageUrl),
            Status: status,
            CreatedAtUtc: nowUtc,
            UpdatedAtUtc: nowUtc,
            PublishedAtUtc: publishedAt);
    }

    /// <summary>
    /// What: Applies a validated edit request to an existing editorial article.
    /// How: normalizes all editable fields, regenerates the slug from the new
    /// title, preserves immutable creation data, and updates workflow timestamps.
    /// Why: edit behavior should be centralized so controllers and stores cannot
    /// accidentally persist inconsistent article content.
    /// </summary>
    public static EditorialArticle Update(EditorialArticle existing, EditorialArticleRequest request, DateTimeOffset nowUtc)
    {
        var title = RequireText(request.Title, "Title", 5, 180);
        var status = NormalizeStatus(request.Status ?? existing.Status.ToString());
        var publishedAt = status == EditorialArticleStatus.Published
            ? existing.PublishedAtUtc ?? nowUtc
            : existing.PublishedAtUtc;

        return existing with
        {
            Title = title,
            Slug = CreateSlug(title),
            Summary = RequireText(request.Summary, "Summary", 10, 500),
            Body = RequireText(request.Body, "Body", 50, 20000),
            Author = RequireText(request.Author, "Author", 2, 120),
            Category = NormalizeCategory(request.Category),
            ImageUrl = EmptyToNull(request.ImageUrl),
            Status = status,
            UpdatedAtUtc = nowUtc,
            PublishedAtUtc = publishedAt
        };
    }

    /// <summary>
    /// What: Changes only the workflow status of an editorial article.
    /// How: copies the existing article, replaces Status and UpdatedAtUtc, and
    /// sets PublishedAtUtc the first time an article becomes published.
    /// Why: status changes are separate editorial workflow actions and should not
    /// require the frontend to resubmit the whole article body.
    /// </summary>
    public static EditorialArticle ChangeStatus(EditorialArticle existing, EditorialArticleStatus status, DateTimeOffset nowUtc)
    {
        return existing with
        {
            Status = status,
            UpdatedAtUtc = nowUtc,
            PublishedAtUtc = status == EditorialArticleStatus.Published
                ? existing.PublishedAtUtc ?? nowUtc
                : existing.PublishedAtUtc
        };
    }

    /// <summary>
    /// What: Validates required editorial text fields.
    /// How: trims the supplied value and checks it against the field-specific
    /// minimum and maximum length.
    /// Why: article title, summary, body, and author should fail fast with useful
    /// messages before a domain object is created or updated.
    /// </summary>
    private static string RequireText(string? value, string fieldName, int minLength, int maxLength)
    {
        var normalized = value?.Trim() ?? string.Empty;

        if (normalized.Length < minLength || normalized.Length > maxLength)
        {
            throw new ArgumentException($"{fieldName} must be between {minLength} and {maxLength} characters.");
        }

        return normalized;
    }

    /// <summary>
    /// What: Converts an editorial category into the stored canonical value.
    /// How: trims/lowercases the text and verifies it exists in the allowed
    /// category set shared with the news UI.
    /// Why: consistent category values make filtering, display, and future reports
    /// reliable without needing to handle spelling variations.
    /// </summary>
    private static string NormalizeCategory(string? category)
    {
        var normalized = category?.Trim().ToLowerInvariant() ?? string.Empty;

        if (!AllowedCategories.Contains(normalized))
        {
            throw new ArgumentException("Category must be one of: business, entertainment, general, health, science, sports, technology.");
        }

        return normalized;
    }

    /// <summary>
    /// What: Converts optional status text into the editorial status enum.
    /// How: blank status defaults to Draft and nonblank status is parsed
    /// case-insensitively against EditorialArticleStatus.
    /// Why: the API contract can stay string-friendly for JSON while the domain
    /// keeps a strongly typed workflow state.
    /// </summary>
    private static EditorialArticleStatus NormalizeStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return EditorialArticleStatus.Draft;
        }

        return Enum.TryParse<EditorialArticleStatus>(status, ignoreCase: true, out var parsed)
            ? parsed
            : throw new ArgumentException("Status must be one of: Draft, Review, Published, Archived.");
    }

    /// <summary>
    /// What: Builds a URL-safe slug from an article title.
    /// How: lowercases the title, replaces non-alphanumeric runs with dashes,
    /// removes duplicate dashes, and falls back to a generated id if nothing remains.
    /// Why: public article URLs should be readable and stable enough for display
    /// without storing unsafe punctuation or whitespace.
    /// </summary>
    private static string CreateSlug(string title)
    {
        var slug = SlugUnsafeCharacters().Replace(title.Trim().ToLowerInvariant(), "-");
        slug = SlugDuplicateDashes().Replace(slug, "-").Trim('-');
        return string.IsNullOrWhiteSpace(slug) ? Guid.NewGuid().ToString("N") : slug;
    }

    /// <summary>
    /// What: Converts optional blank strings into null values.
    /// How: whitespace-only input returns null; otherwise the trimmed text is kept.
    /// Why: persistence and DTO mapping are easier when optional fields have one
    /// representation for "not supplied" instead of mixing null and empty text.
    /// </summary>
    private static string? EmptyToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex SlugUnsafeCharacters();

    [GeneratedRegex("-+")]
    private static partial Regex SlugDuplicateDashes();
}
