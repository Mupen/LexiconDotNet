using ReactNews.Application.Contracts.Auth;
using ReactNews.Application.Contracts.Articles;
using ReactNews.Application.Contracts.EditorialArticles;
using ReactNews.Application.Contracts.ReaderPreferences;
using ReactNews.Application.Contracts.SavedArticles;
using ReactNews.Application.Contracts.Sources;
using ReactNews.Domain.Entities.Articles;
using ReactNews.Domain.Entities.EditorialArticles;
using ReactNews.Domain.Entities.ReaderPreferences;
using ReactNews.Domain.Entities.SavedArticles;
using ReactNews.Domain.Entities.Sources;
using ReactNews.Domain.Entities.Users;

namespace ReactNews.Application.Mapping;

/// <summary>
/// Maps domain models into API-facing application DTOs.
/// </summary>
/// <remarks>
/// What: these methods perform simple, explicit shape changes.
/// Why: keeping mapping outside controllers prevents controllers from becoming
/// mixed presentation/application code, and keeping mapping outside domain keeps
/// the domain independent of HTTP response contracts.
/// How: use cases call these methods before returning results to the API layer.
/// </remarks>
public static class NewsMapping
{
    /// <summary>
    /// Maps a domain Article to the frontend-facing article DTO.
    /// </summary>
    /// <remarks>
    /// What: copies article fields into NewsArticleDto.
    /// How: the method is an extension method so use cases can write
    /// article.ToDto().
    /// Why: explicit mapping keeps DTO shape independent from domain entity shape
    /// and avoids returning domain records directly from API endpoints.
    /// </remarks>
    public static NewsArticleDto ToDto(this Article article)
    {
        return new NewsArticleDto(
            Id: article.Id,
            SourceName: article.SourceName,
            Author: article.Author,
            Title: article.Title,
            Description: article.Description,
            Url: article.Url,
            ImageUrl: article.ImageUrl,
            PublishedAt: article.PublishedAt,
            Content: article.Content);
    }

    /// <summary>
    /// Maps a domain Source to the frontend-facing source DTO.
    /// </summary>
    /// <remarks>
    /// What: copies source metadata into SourceDto.
    /// How: this extension method performs a direct field-to-field projection.
    /// Why: even simple mappings are kept explicit so future DTO changes do not
    /// accidentally change domain entities or provider contracts.
    /// </remarks>
    public static SourceDto ToDto(this Source source)
    {
        return new SourceDto(
            Id: source.Id,
            Name: source.Name,
            Description: source.Description,
            Url: source.Url,
            Category: source.Category,
            Language: source.Language,
            Country: source.Country);
    }

    /// <summary>
    /// Maps a saved article domain object into the API/application DTO shape.
    /// </summary>
    /// <remarks>
    /// What: copies the saved article's normalized article data plus SavedAtUtc.
    /// How: it reads values from SavedArticle.Article and places SavedAtUtc beside them in the response DTO.
    /// Why: the frontend should receive one flat saved-article object instead of needing to understand backend domain nesting.
    /// </remarks>
    public static SavedArticleDto ToDto(this SavedArticle savedArticle)
    {
        return new SavedArticleDto(
            Id: savedArticle.Article.Id,
            SourceName: savedArticle.Article.SourceName,
            Author: savedArticle.Article.Author,
            Title: savedArticle.Article.Title,
            Description: savedArticle.Article.Description,
            Url: savedArticle.Article.Url,
            ImageUrl: savedArticle.Article.ImageUrl,
            PublishedAt: savedArticle.Article.PublishedAt,
            Content: savedArticle.Article.Content,
            SavedAtUtc: savedArticle.SavedAtUtc);
    }

    /// <summary>
    /// What: Maps reader preference domain data into the API DTO shape.
    /// How: Copies scalar values and category collection into ReaderPreferencesDto.
    /// Why: Keeping this mapping explicit protects the domain from API contract changes.
    /// </summary>
    public static ReaderPreferencesDto ToDto(this ReaderPreferences preferences)
    {
        return new ReaderPreferencesDto(
            Theme: preferences.Theme,
            FontScale: preferences.FontScale,
            CompactCards: preferences.CompactCards,
            PreferredCategories: preferences.PreferredCategories);
    }

    /// <summary>
    /// What: Maps an editorial article domain object into the API DTO.
    /// How: Copies all article fields and converts the status enum to a string.
    /// Why: The frontend should display readable status values while the domain keeps a strict enum.
    /// </summary>
    public static EditorialArticleDto ToDto(this EditorialArticle article)
    {
        return new EditorialArticleDto(
            Id: article.Id,
            Title: article.Title,
            Slug: article.Slug,
            Summary: article.Summary,
            Body: article.Body,
            Author: article.Author,
            Category: article.Category,
            ImageUrl: article.ImageUrl,
            Status: article.Status.ToString(),
            CreatedAtUtc: article.CreatedAtUtc,
            UpdatedAtUtc: article.UpdatedAtUtc,
            PublishedAtUtc: article.PublishedAtUtc);
    }

    /// <summary>
    /// What: Maps a user domain object into a safe auth DTO.
    /// How: Copies identity fields and converts the role enum to text while omitting PasswordHash.
    /// Why: The frontend needs account identity but must never receive credential storage data.
    /// </summary>
    public static AuthUserDto ToDto(this User user)
    {
        return new AuthUserDto(
            Id: user.Id,
            Email: user.Email,
            DisplayName: user.DisplayName,
            Role: user.Role.ToString());
    }
}
