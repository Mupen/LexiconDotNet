namespace ReactNews.Domain.Enums.Users;

/// <summary>
/// What: Represents the authorization role assigned to a ReactNews account.
/// How: Reader can use private reader features; Admin can also manage editorial content.
/// Why: Roles are the simplest professional boundary between normal account behavior and newsroom/admin behavior.
/// </summary>
public enum UserRole
{
    Reader = 0,
    Admin = 1
}
