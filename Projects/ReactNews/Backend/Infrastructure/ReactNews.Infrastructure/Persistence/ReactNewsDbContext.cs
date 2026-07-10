using Microsoft.EntityFrameworkCore;
using ReactNews.Infrastructure.Persistence.Entities;

namespace ReactNews.Infrastructure.Persistence;

/// <summary>
/// EF Core database context for ReactNews infrastructure data.
/// </summary>
/// <remarks>
/// What: represents the SQLite database used by the backend.
/// Why: article snapshots must survive backend restarts before article detail
/// pages, saved articles, and editorial workflows can become professional.
/// How: infrastructure maps persistence records to database tables while the
/// application layer continues to depend on interfaces and domain entities.
/// </remarks>
public sealed class ReactNewsDbContext : DbContext
{
    public ReactNewsDbContext(DbContextOptions<ReactNewsDbContext> options)
        : base(options)
    {
    }

    public DbSet<ArticleSnapshotRecord> ArticleSnapshots => Set<ArticleSnapshotRecord>();

    public DbSet<SavedArticleRecord> SavedArticles => Set<SavedArticleRecord>();

    public DbSet<ReaderPreferencesRecord> ReaderPreferences => Set<ReaderPreferencesRecord>();

    public DbSet<EditorialArticleRecord> EditorialArticles => Set<EditorialArticleRecord>();

    public DbSet<UserRecord> Users => Set<UserRecord>();

    /// <summary>
    /// Configures the EF Core database model.
    /// </summary>
    /// <remarks>
    /// What: defines the ArticleSnapshots table shape, required fields, max
    /// lengths, and indexes.
    /// How: Fluent API is used instead of relying only on conventions.
    /// Why: explicit configuration teaches what the database schema looks like and
    /// prevents accidental unlimited text columns or missing indexes.
    /// </remarks>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ArticleSnapshotRecord>(entity =>
        {
            entity.ToTable("ArticleSnapshots");

            entity.HasKey(article => article.Id);

            entity.Property(article => article.Id)
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(article => article.SourceName)
                .HasMaxLength(300);

            entity.Property(article => article.Author)
                .HasMaxLength(300);

            entity.Property(article => article.Title)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(article => article.Description)
                .HasMaxLength(2000);

            entity.Property(article => article.Url)
                .HasMaxLength(2000)
                .IsRequired();

            entity.Property(article => article.ImageUrl)
                .HasMaxLength(2000);

            entity.Property(article => article.Content)
                .HasMaxLength(4000);

            entity.HasIndex(article => article.ExpiresAtUnixTimeMilliseconds);
            entity.HasIndex(article => article.Url);
        });

        modelBuilder.Entity<SavedArticleRecord>(entity =>
        {
            entity.ToTable("SavedArticles");

            entity.HasKey(article => article.Id);

            entity.Property(article => article.Id)
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(article => article.UserId)
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(article => article.ArticleId)
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(article => article.SourceName)
                .HasMaxLength(300);

            entity.Property(article => article.Author)
                .HasMaxLength(300);

            entity.Property(article => article.Title)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(article => article.Description)
                .HasMaxLength(2000);

            entity.Property(article => article.Url)
                .HasMaxLength(2000)
                .IsRequired();

            entity.Property(article => article.ImageUrl)
                .HasMaxLength(2000);

            entity.Property(article => article.Content)
                .HasMaxLength(4000);

            entity.HasIndex(article => article.SavedAtUnixTimeMilliseconds);
            entity.HasIndex(article => article.Url);
            entity.HasIndex(article => new { article.UserId, article.ArticleId })
                .IsUnique();
        });

        modelBuilder.Entity<ReaderPreferencesRecord>(entity =>
        {
            entity.ToTable("ReaderPreferences");

            entity.HasKey(preferences => preferences.Id);

            entity.Property(preferences => preferences.Id)
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(preferences => preferences.UserId)
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(preferences => preferences.Theme)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(preferences => preferences.FontScale)
                .HasPrecision(4, 2)
                .IsRequired();

            entity.Property(preferences => preferences.PreferredCategories)
                .HasMaxLength(500)
                .IsRequired();

            entity.HasIndex(preferences => preferences.UserId)
                .IsUnique();
        });

        modelBuilder.Entity<EditorialArticleRecord>(entity =>
        {
            entity.ToTable("EditorialArticles");

            entity.HasKey(article => article.Id);

            entity.Property(article => article.Id)
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(article => article.Title)
                .HasMaxLength(180)
                .IsRequired();

            entity.Property(article => article.Slug)
                .HasMaxLength(220)
                .IsRequired();

            entity.Property(article => article.Summary)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(article => article.Body)
                .HasMaxLength(20000)
                .IsRequired();

            entity.Property(article => article.Author)
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(article => article.Category)
                .HasMaxLength(40)
                .IsRequired();

            entity.Property(article => article.ImageUrl)
                .HasMaxLength(2000);

            entity.Property(article => article.Status)
                .HasMaxLength(30)
                .IsRequired();

            entity.HasIndex(article => article.Slug);
            entity.HasIndex(article => article.Status);
            entity.HasIndex(article => article.UpdatedAtUnixTimeMilliseconds);
        });

        modelBuilder.Entity<UserRecord>(entity =>
        {
            entity.ToTable("Users");

            entity.HasKey(user => user.Id);

            entity.Property(user => user.Id)
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(user => user.Email)
                .HasMaxLength(320)
                .IsRequired();

            entity.Property(user => user.DisplayName)
                .HasMaxLength(80)
                .IsRequired();

            entity.Property(user => user.Role)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(user => user.PasswordHash)
                .HasMaxLength(300)
                .IsRequired();

            entity.HasIndex(user => user.Email)
                .IsUnique();
        });
    }
}
