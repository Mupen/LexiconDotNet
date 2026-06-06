using DataDrivenCaching.Domain.Entities;
using DataDrivenCaching.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DataDrivenCaching.Infrastructure.DataStores;

// WHAT:
// LabDataStore is the concrete data access object for the lab's durable records.
//
// WHY:
// This project is intentionally data-driven and educational. We do not add an
// interface just because a class talks to storage. An interface would be useful
// if we had multiple real storage implementations, a meaningful test seam, or a
// plugin boundary. Right now there is one real storage choice: SQLite through
// EF Core. A concrete class makes that choice easier to see.
//
// DATA DESIGN:
// The data returned from this class is authoritative backend data. Browser
// storage and cache demos may hold copies, but those copies should be compared
// back to this store when teaching stale data and trust boundaries.
public sealed class LabDataStore(DataDrivenCachingDbContext dbContext)
{
    public async Task<IReadOnlyList<LabDataRecord>> GetRecordsAsync(CancellationToken cancellationToken)
    {
        return await dbContext.DataRecords
            .OrderBy(record => record.Name)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<LabDataRecord> UpsertRecordAsync(
        string name,
        string value,
        CancellationToken cancellationToken)
    {
        var record = await dbContext.DataRecords
            .SingleOrDefaultAsync(existing => existing.Name == name, cancellationToken);

        if (record is null)
        {
            record = new LabDataRecord
            {
                Name = name,
                Value = value,
                UpdatedAtUtc = DateTimeOffset.UtcNow
            };

            dbContext.DataRecords.Add(record);
        }
        else
        {
            record.Value = value;
            record.UpdatedAtUtc = DateTimeOffset.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return record;
    }
}
