using ReactNews.Application.Interfaces;
using ReactNews.Domain.Entities.Users;
using ReactNews.Domain.Enums.Users;
using ReactNews.Infrastructure.Persistence;
using ReactNews.Infrastructure.Persistence.Entities;

namespace ReactNews.Infrastructure.Storage.Users;

/// <summary>
/// What: EF Core implementation of user account persistence.
/// How: Maps between User domain objects and UserRecord rows.
/// Why: Application owns auth rules while Infrastructure owns SQLite storage.
/// </summary>
public sealed class EfUserStore : IUserStore
{
    private readonly ReactNewsDbContext _dbContext;

    public EfUserStore(ReactNewsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// What: Finds a user account by email address.
    /// How: normalizes the supplied email to the stored lowercase form and queries
    /// the Users table for a single matching record.
    /// Why: login and registration need consistent case-insensitive email lookup
    /// without exposing EF records to Application.
    /// </summary>
    public User? FindByEmail(string email)
    {
        var normalized = email.Trim().ToLowerInvariant();
        var record = _dbContext.Users.SingleOrDefault(user => user.Email == normalized);
        return record is null ? null : ToDomain(record);
    }

    /// <summary>
    /// What: Finds a user account by its stable account id.
    /// How: uses EF Core primary-key lookup and maps the found row to the domain
    /// User record.
    /// Why: authenticated requests carry the account id claim, so user-specific
    /// use cases need a direct id lookup.
    /// </summary>
    public User? FindById(string id)
    {
        var record = _dbContext.Users.Find(id);
        return record is null ? null : ToDomain(record);
    }

    /// <summary>
    /// What: Inserts or updates one user account.
    /// How: finds the row by id, creates it when missing, copies all account
    /// fields, and commits changes through EF Core.
    /// Why: Application should be able to persist registration, profile, password,
    /// and seed changes through one store operation.
    /// </summary>
    public User Save(User user)
    {
        var record = _dbContext.Users.Find(user.Id);

        if (record is null)
        {
            record = new UserRecord { Id = user.Id };
            _dbContext.Users.Add(record);
        }

        record.Email = user.Email;
        record.DisplayName = user.DisplayName;
        record.Role = user.Role.ToString();
        record.PasswordHash = user.PasswordHash;
        record.CreatedAtUnixTimeMilliseconds = user.CreatedAtUtc.ToUnixTimeMilliseconds();

        _dbContext.SaveChanges();

        return ToDomain(record);
    }

    /// <summary>
    /// What: Deletes one user account by id.
    /// How: finds the row, removes it when present, saves changes, and reports
    /// whether a row was actually deleted.
    /// Why: account deletion needs a clear success/not-found result so the API can
    /// return the correct response to the frontend.
    /// </summary>
    public bool Delete(string id)
    {
        var record = _dbContext.Users.Find(id);

        if (record is null)
        {
            return false;
        }

        _dbContext.Users.Remove(record);
        _dbContext.SaveChanges();
        return true;
    }

    /// <summary>
    /// What: Converts an EF user row into the domain User record.
    /// How: parses the stored role text into UserRole and converts the stored Unix
    /// timestamp back to DateTimeOffset.
    /// Why: Application should operate on the domain model and not depend on EF
    /// persistence record shapes.
    /// </summary>
    private static User ToDomain(UserRecord record)
    {
        return new User(
            Id: record.Id,
            Email: record.Email,
            DisplayName: record.DisplayName,
            Role: Enum.Parse<UserRole>(record.Role),
            PasswordHash: record.PasswordHash,
            CreatedAtUtc: DateTimeOffset.FromUnixTimeMilliseconds(record.CreatedAtUnixTimeMilliseconds));
    }
}
