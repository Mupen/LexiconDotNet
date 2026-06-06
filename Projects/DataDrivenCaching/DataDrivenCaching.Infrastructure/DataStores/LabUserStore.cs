using DataDrivenCaching.Domain.Entities;
using DataDrivenCaching.Infrastructure.Persistence;
using DataDrivenCaching.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace DataDrivenCaching.Infrastructure.DataStores;

// WHAT:
// LabUserStore is the concrete account data access object for the demo.
//
// WHY:
// Login and session lessons need real backend-owned account data. We keep this
// concrete instead of hiding it behind an interface because there is currently
// one real storage choice: SQLite through EF Core.
//
// DATA DESIGN:
// Users are authoritative backend data. The browser may ask "who am I?" after
// login, but the browser must never be trusted to decide that identity itself.
public sealed class LabUserStore(DataDrivenCachingDbContext dbContext)
{
    public async Task<LabUser?> FindByUserNameAsync(
        string userName,
        CancellationToken cancellationToken)
    {
        return await dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(user => user.UserName == userName, cancellationToken);
    }

    public async Task SeedDemoUsersAsync(CancellationToken cancellationToken = default)
    {
        if (await dbContext.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;

        dbContext.Users.AddRange(
            new LabUser
            {
                UserName = "alice",
                DisplayName = "Alice Authority",
                PasswordHash = PasswordHashing.HashPassword("Password123!"),
                CreatedAtUtc = now
            },
            new LabUser
            {
                UserName = "bob",
                DisplayName = "Bob Browser",
                PasswordHash = PasswordHashing.HashPassword("Password123!"),
                CreatedAtUtc = now
            },
            new LabUser
            {
                UserName = "charlie",
                DisplayName = "Charlie Cache",
                PasswordHash = PasswordHashing.HashPassword("Password123!"),
                CreatedAtUtc = now
            });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
