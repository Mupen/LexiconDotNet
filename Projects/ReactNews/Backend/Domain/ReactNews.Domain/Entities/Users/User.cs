using ReactNews.Domain.Enums.Users;

namespace ReactNews.Domain.Entities.Users;

/// <summary>
/// What: Represents one ReactNews account.
/// How: Stores identity fields, role, password hash, and creation timestamp.
/// Why: Accounts are needed so reader features and admin/editorial features can be separated correctly.
/// </summary>
public sealed record User(
    string Id,
    string Email,
    string DisplayName,
    UserRole Role,
    string PasswordHash,
    DateTimeOffset CreatedAtUtc);
