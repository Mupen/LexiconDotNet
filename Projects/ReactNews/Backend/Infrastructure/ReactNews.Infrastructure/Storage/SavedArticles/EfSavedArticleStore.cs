using ReactNews.Application.Interfaces;
using ReactNews.Domain.Entities.Articles;
using ReactNews.Domain.Entities.SavedArticles;
using ReactNews.Infrastructure.Persistence;
using ReactNews.Infrastructure.Persistence.Entities;

namespace ReactNews.Infrastructure.Storage.SavedArticles;

/// <summary>
/// What: EF Core implementation of saved-article persistence.
/// How: Maps between domain SavedArticle objects and SavedArticleRecord rows in SQLite.
/// Why: Application should depend on ISavedArticleStore while Infrastructure owns the database schema and EF Core behavior.
/// </summary>
public sealed class EfSavedArticleStore : ISavedArticleStore
{
    private readonly ReactNewsDbContext _dbContext;

    public EfSavedArticleStore(ReactNewsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// What: Saves or refreshes one article in one user's saved list.
    /// How: Finds an existing record by user id plus article id; inserts when missing or updates fields when already saved.
    /// Why: The save button should be idempotent per user, while different users can save the same external article independently.
    /// </summary>
    public SavedArticle Save(string userId, Article article, DateTimeOffset savedAtUtc)
    {
        var record = _dbContext.SavedArticles.Find(ToRecordId(userId, article.Id));

        if (record is null)
        {
            record = ToRecord(userId, article, savedAtUtc);
            _dbContext.SavedArticles.Add(record);
        }
        else
        {
            UpdateRecord(record, userId, article, savedAtUtc);
        }

        _dbContext.SaveChanges();

        return ToSavedArticle(record);
    }

    /// <summary>
    /// What: Lists all saved articles for one reader.
    /// How: Orders records by saved timestamp descending and maps each row to a domain SavedArticle.
    /// Why: A reader normally expects the most recently saved article to appear first.
    /// </summary>
    public IReadOnlyList<SavedArticle> List(string userId)
    {
        return _dbContext.SavedArticles
            .Where(article => article.UserId == userId)
            .OrderByDescending(article => article.SavedAtUnixTimeMilliseconds)
            .Select(article => ToSavedArticle(article))
            .ToList();
    }

    /// <summary>
    /// What: Removes one saved article by id.
    /// How: Finds the record, removes it when present, saves changes, and returns whether removal happened.
    /// Why: The API needs a reliable success/not-found result for DELETE requests.
    /// </summary>
    public bool Remove(string userId, string articleId)
    {
        var record = _dbContext.SavedArticles.Find(ToRecordId(userId, articleId));

        if (record is null)
        {
            return false;
        }

        _dbContext.SavedArticles.Remove(record);
        _dbContext.SaveChanges();

        return true;
    }

    /// <summary>
    /// What: Converts a domain Article into a saved-article database row.
    /// How: Copies article fields and stores the saved timestamp as Unix milliseconds.
    /// Why: Unix milliseconds are easy to sort/filter in SQLite and easy to convert back into DateTimeOffset.
    /// </summary>
    private static SavedArticleRecord ToRecord(string userId, Article article, DateTimeOffset savedAtUtc)
    {
        var record = new SavedArticleRecord();
        UpdateRecord(record, userId, article, savedAtUtc);
        return record;
    }

    /// <summary>
    /// What: Updates a persistence row with current article data.
    /// How: Copies all article fields plus the latest saved timestamp onto the record.
    /// Why: External article metadata can differ between fetches, so save should preserve the newest data ReactNews has seen.
    /// </summary>
    private static void UpdateRecord(
        SavedArticleRecord record,
        string userId,
        Article article,
        DateTimeOffset savedAtUtc)
    {
        record.Id = ToRecordId(userId, article.Id);
        record.UserId = userId;
        record.ArticleId = article.Id;
        record.SourceName = article.SourceName;
        record.Author = article.Author;
        record.Title = article.Title;
        record.Description = article.Description;
        record.Url = article.Url;
        record.ImageUrl = article.ImageUrl;
        record.PublishedAt = article.PublishedAt;
        record.Content = article.Content;
        record.SavedAtUnixTimeMilliseconds = savedAtUtc.ToUnixTimeMilliseconds();
    }

    /// <summary>
    /// What: Converts a database row back into a domain SavedArticle.
    /// How: Rebuilds the nested Article entity and converts Unix milliseconds back to UTC time.
    /// Why: Domain and Application should work with meaningful types, not persistence records.
    /// </summary>
    private static SavedArticle ToSavedArticle(SavedArticleRecord record)
    {
        var article = new Article(
            Id: record.ArticleId,
            SourceName: record.SourceName,
            Author: record.Author,
            Title: record.Title,
            Description: record.Description,
            Url: record.Url,
            ImageUrl: record.ImageUrl,
            PublishedAt: record.PublishedAt,
            Content: record.Content);

        return new SavedArticle(
            Article: article,
            SavedAtUtc: DateTimeOffset.FromUnixTimeMilliseconds(record.SavedAtUnixTimeMilliseconds));
    }

    /// <summary>
    /// What: Builds the primary key for one user's saved article row.
    /// How: combines the user id and article id into a deterministic string.
    /// Why: the same external article can be saved by many users, so the database
    /// key must include both the owner and the article snapshot id.
    /// </summary>
    private static string ToRecordId(string userId, string articleId)
    {
        return $"{userId}:{articleId}";
    }
}
