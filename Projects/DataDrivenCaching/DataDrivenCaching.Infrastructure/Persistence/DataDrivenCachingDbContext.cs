using DataDrivenCaching.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataDrivenCaching.Infrastructure.Persistence;

// WHAT:
// DataDrivenCachingDbContext maps domain entities to SQLite database tables.
//
// WHY:
// SQLite gives this learning project durable backend storage without requiring
// a separate database server. That makes it ideal for showing the difference
// between authoritative backend data and temporary/cached copies.
//
// DATA DESIGN:
// Data in this DbContext is backend-owned and durable. It survives browser
// refresh, browser restart, and server memory cache expiration. It also survives
// server restart because the data is written to a SQLite file on disk.
public sealed class DataDrivenCachingDbContext(DbContextOptions<DataDrivenCachingDbContext> options)
    : DbContext(options)
{
    public DbSet<LabUser> Users => Set<LabUser>();

    public DbSet<LabDataRecord> DataRecords => Set<LabDataRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LabUser>(entity =>
        {
            entity.HasKey(user => user.Id);
            entity.Property(user => user.UserName).HasMaxLength(80).IsRequired();
            entity.Property(user => user.DisplayName).HasMaxLength(120).IsRequired();
            entity.Property(user => user.PasswordHash).HasMaxLength(512).IsRequired();
            entity.HasIndex(user => user.UserName).IsUnique();
        });

        modelBuilder.Entity<LabDataRecord>(entity =>
        {
            entity.HasKey(record => record.Id);
            entity.Property(record => record.Name).HasMaxLength(120).IsRequired();
            entity.Property(record => record.Value).HasMaxLength(1_000).IsRequired();
            entity.HasIndex(record => record.Name).IsUnique();
        });
    }
}
