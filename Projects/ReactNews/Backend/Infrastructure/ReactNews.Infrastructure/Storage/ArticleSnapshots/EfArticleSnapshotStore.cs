using Microsoft.EntityFrameworkCore;
using ReactNews.Application.Interfaces;
using ReactNews.Domain.Entities.Articles;
using ReactNews.Infrastructure.Persistence;
using ReactNews.Infrastructure.Persistence.Entities;

namespace ReactNews.Infrastructure.Storage.ArticleSnapshots;

/// <summary>
/// SQLite-backed article snapshot storage.
/// </summary>
/// <remarks>
/// What: stores article snapshots in the ReactNews database.
/// Why: the old in-memory store lost detail-page data every time the backend
/// stopped. Persisting snapshots is the first step toward saved articles,
/// personal feeds, and admin/editorial features.
/// How: each fetched article is upserted by generated id. Expired rows are
/// removed lazily during writes and reads so the table does not grow forever.
/// </remarks>
public sealed class EfArticleSnapshotStore : IArticleSnapshotStore
{
    private readonly ReactNewsDbContext _dbContext;

    public EfArticleSnapshotStore(ReactNewsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Saves or updates article snapshots.
    /// </summary>
    /// <remarks>
    /// What: persists the articles returned by a list/search request.
    /// How: expired snapshots are removed first, then each incoming article is
    /// either inserted or used to update an existing row with the same generated
    /// id.
    /// Why: detail pages need article metadata after navigation or backend
    /// restart, and upserting avoids duplicate rows for the same article.
    /// </remarks>
    public void Remember(IEnumerable<Article> articles, DateTimeOffset expiresAtUtc)
    {
        var now = DateTimeOffset.UtcNow;
        var nowUnixTimeMilliseconds = now.ToUnixTimeMilliseconds();
        var articleList = articles.ToList();

        RemoveExpiredSnapshots(nowUnixTimeMilliseconds);

        foreach (var article in articleList)
        {
            var existingRecord = _dbContext.ArticleSnapshots.Find(article.Id);

            if (existingRecord is null)
            {
                _dbContext.ArticleSnapshots.Add(ToRecord(article, expiresAtUtc, now));
                continue;
            }

            UpdateRecord(existingRecord, article, expiresAtUtc, now);
        }

        _dbContext.SaveChanges();
    }

    /// <summary>
    /// Finds a stored article snapshot by generated id.
    /// </summary>
    /// <remarks>
    /// What: returns the article when a non-expired snapshot exists.
    /// How: EF Core loads the record without tracking for normal reads. Expired
    /// records are loaded with tracking, removed, and saved before returning null.
    /// Why: reads should be cheap, and expired snapshot cleanup should happen
    /// lazily without requiring a background worker yet.
    /// </remarks>
    public Article? Find(string id)
    {
        var now = DateTimeOffset.UtcNow;
        var record = _dbContext.ArticleSnapshots
            .AsNoTracking()
            .SingleOrDefault(article => article.Id == id);

        if (record is null)
        {
            return null;
        }

        if (record.ExpiresAtUnixTimeMilliseconds <= now.ToUnixTimeMilliseconds())
        {
            var expiredRecord = _dbContext.ArticleSnapshots
                .Where(article => article.Id == id)
                .SingleOrDefault();

            if (expiredRecord is not null)
            {
                _dbContext.ArticleSnapshots.Remove(expiredRecord);
                _dbContext.SaveChanges();
            }

            return null;
        }

        return ToArticle(record);
    }

    /// <summary>
    /// Deletes expired snapshot rows.
    /// </summary>
    /// <remarks>
    /// What: removes rows whose expiry timestamp is older than the current time.
    /// How: timestamps are stored as Unix milliseconds so SQLite can compare them
    /// as numbers.
    /// Why: SQLite has awkward DateTimeOffset translation behavior. Numeric time
    /// values are simple, indexable, and easy to test.
    /// </remarks>
    private void RemoveExpiredSnapshots(long nowUnixTimeMilliseconds)
    {
        var expiredSnapshots = _dbContext.ArticleSnapshots
            .Where(article => article.ExpiresAtUnixTimeMilliseconds <= nowUnixTimeMilliseconds)
            .ToList();

        if (expiredSnapshots.Count == 0)
        {
            return;
        }

        _dbContext.ArticleSnapshots.RemoveRange(expiredSnapshots);
        _dbContext.SaveChanges();
    }

    /// <summary>
    /// Converts a domain article into a persistence record.
    /// </summary>
    /// <remarks>
    /// What: copies article fields into the EF Core record type.
    /// How: DateTimeOffset expiry/stored values become Unix milliseconds for
    /// reliable SQLite comparison.
    /// Why: keeping conversion here prevents EF Core persistence concerns from
    /// leaking into the Domain or Application projects.
    /// </remarks>
    private static ArticleSnapshotRecord ToRecord(
        Article article,
        DateTimeOffset expiresAtUtc,
        DateTimeOffset storedAtUtc)
    {
        return new ArticleSnapshotRecord
        {
            Id = article.Id,
            SourceName = article.SourceName,
            Author = article.Author,
            Title = article.Title,
            Description = article.Description,
            Url = article.Url,
            ImageUrl = article.ImageUrl,
            PublishedAt = article.PublishedAt,
            Content = article.Content,
            ExpiresAtUnixTimeMilliseconds = expiresAtUtc.ToUnixTimeMilliseconds(),
            StoredAtUnixTimeMilliseconds = storedAtUtc.ToUnixTimeMilliseconds()
        };
    }

    /// <summary>
    /// Applies the newest article snapshot values to an existing record.
    /// </summary>
    /// <remarks>
    /// What: updates all stored article fields and expiry metadata.
    /// How: the tracked EF Core entity is mutated; SaveChanges later writes those
    /// changes.
    /// Why: NewsAPI data for the same URL/title can change over time, so an
    /// existing snapshot should be refreshed instead of ignored.
    /// </remarks>
    private static void UpdateRecord(
        ArticleSnapshotRecord record,
        Article article,
        DateTimeOffset expiresAtUtc,
        DateTimeOffset storedAtUtc)
    {
        record.SourceName = article.SourceName;
        record.Author = article.Author;
        record.Title = article.Title;
        record.Description = article.Description;
        record.Url = article.Url;
        record.ImageUrl = article.ImageUrl;
        record.PublishedAt = article.PublishedAt;
        record.Content = article.Content;
        record.ExpiresAtUnixTimeMilliseconds = expiresAtUtc.ToUnixTimeMilliseconds();
        record.StoredAtUnixTimeMilliseconds = storedAtUtc.ToUnixTimeMilliseconds();
    }

    /// <summary>
    /// Converts a persistence record back into the domain article shape.
    /// </summary>
    /// <remarks>
    /// What: reconstructs the Article returned to application use cases.
    /// How: only article content fields are copied; persistence-only timestamps
    /// remain inside Infrastructure.
    /// Why: callers need article data, not database bookkeeping details.
    /// </remarks>
    private static Article ToArticle(ArticleSnapshotRecord record)
    {
        return new Article(
            Id: record.Id,
            SourceName: record.SourceName,
            Author: record.Author,
            Title: record.Title,
            Description: record.Description,
            Url: record.Url,
            ImageUrl: record.ImageUrl,
            PublishedAt: record.PublishedAt,
            Content: record.Content);
    }
}
