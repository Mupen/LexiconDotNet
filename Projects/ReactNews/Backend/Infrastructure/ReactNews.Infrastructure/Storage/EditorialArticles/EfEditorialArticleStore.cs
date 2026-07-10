using ReactNews.Application.Interfaces;
using ReactNews.Domain.Entities.EditorialArticles;
using ReactNews.Domain.Enums.EditorialArticles;
using ReactNews.Infrastructure.Persistence;
using ReactNews.Infrastructure.Persistence.Entities;

namespace ReactNews.Infrastructure.Storage.EditorialArticles;

/// <summary>
/// What: EF Core implementation for editorial article persistence.
/// How: Maps between EditorialArticle domain records and EditorialArticleRecord database rows.
/// Why: Infrastructure owns persistence details while Application owns editorial workflow rules.
/// </summary>
public sealed class EfEditorialArticleStore : IEditorialArticleStore
{
    private readonly ReactNewsDbContext _dbContext;

    public EfEditorialArticleStore(ReactNewsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// What: Lists all editorial articles stored in SQLite.
    /// How: orders records by last update time descending and maps each EF record
    /// back into the domain EditorialArticle record.
    /// Why: the admin workspace needs newest-changed content first, independent of
    /// whether the article is draft, review, published, or archived.
    /// </summary>
    public IReadOnlyList<EditorialArticle> List()
    {
        return _dbContext.EditorialArticles
            .OrderByDescending(article => article.UpdatedAtUnixTimeMilliseconds)
            .Select(article => ToDomain(article))
            .ToList();
    }

    /// <summary>
    /// What: Finds one editorial article by its stored id.
    /// How: uses EF Core's primary-key lookup and maps the record to the domain
    /// model when found.
    /// Why: Application should receive null/domain objects and not EF tracking
    /// records or database-specific query behavior.
    /// </summary>
    public EditorialArticle? Find(string id)
    {
        var record = _dbContext.EditorialArticles.Find(id);
        return record is null ? null : ToDomain(record);
    }

    /// <summary>
    /// What: Inserts or updates one editorial article.
    /// How: finds the existing row by id, creates a new row when missing, copies
    /// domain values to the row, and commits changes through EF Core.
    /// Why: the Application layer should use one save operation without knowing
    /// whether the article is new or already persisted.
    /// </summary>
    public EditorialArticle Save(EditorialArticle article)
    {
        var record = _dbContext.EditorialArticles.Find(article.Id);

        if (record is null)
        {
            record = new EditorialArticleRecord { Id = article.Id };
            _dbContext.EditorialArticles.Add(record);
        }

        UpdateRecord(record, article);
        _dbContext.SaveChanges();

        return ToDomain(record);
    }

    /// <summary>
    /// What: Copies domain editorial article values into an EF persistence record.
    /// How: assigns scalar fields directly, converts enum status to text, and
    /// converts DateTimeOffset timestamps to Unix milliseconds.
    /// Why: keeping mapping in one helper prevents insert and update paths from
    /// drifting apart.
    /// </summary>
    private static void UpdateRecord(EditorialArticleRecord record, EditorialArticle article)
    {
        record.Id = article.Id;
        record.Title = article.Title;
        record.Slug = article.Slug;
        record.Summary = article.Summary;
        record.Body = article.Body;
        record.Author = article.Author;
        record.Category = article.Category;
        record.ImageUrl = article.ImageUrl;
        record.Status = article.Status.ToString();
        record.CreatedAtUnixTimeMilliseconds = article.CreatedAtUtc.ToUnixTimeMilliseconds();
        record.UpdatedAtUnixTimeMilliseconds = article.UpdatedAtUtc.ToUnixTimeMilliseconds();
        record.PublishedAtUnixTimeMilliseconds = article.PublishedAtUtc?.ToUnixTimeMilliseconds();
    }

    /// <summary>
    /// What: Converts an EF editorial article row into the domain model.
    /// How: parses stored status text back to the enum and converts Unix
    /// milliseconds back to DateTimeOffset values.
    /// Why: upper layers should work with domain concepts instead of persistence
    /// storage formats.
    /// </summary>
    private static EditorialArticle ToDomain(EditorialArticleRecord record)
    {
        return new EditorialArticle(
            Id: record.Id,
            Title: record.Title,
            Slug: record.Slug,
            Summary: record.Summary,
            Body: record.Body,
            Author: record.Author,
            Category: record.Category,
            ImageUrl: record.ImageUrl,
            Status: Enum.Parse<EditorialArticleStatus>(record.Status),
            CreatedAtUtc: DateTimeOffset.FromUnixTimeMilliseconds(record.CreatedAtUnixTimeMilliseconds),
            UpdatedAtUtc: DateTimeOffset.FromUnixTimeMilliseconds(record.UpdatedAtUnixTimeMilliseconds),
            PublishedAtUtc: record.PublishedAtUnixTimeMilliseconds is null
                ? null
                : DateTimeOffset.FromUnixTimeMilliseconds(record.PublishedAtUnixTimeMilliseconds.Value));
    }
}
