namespace ReactNews.Infrastructure.Persistence.Entities;

/// <summary>
/// What: Database row for one ReactNews user account.
/// How: Stores identity fields, role text, password hash, and created timestamp.
/// Why: User accounts need durable persistence so cookie sessions can map back to real users.
/// </summary>
public sealed class UserRecord
{
    public string Id { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public long CreatedAtUnixTimeMilliseconds { get; set; }
}
