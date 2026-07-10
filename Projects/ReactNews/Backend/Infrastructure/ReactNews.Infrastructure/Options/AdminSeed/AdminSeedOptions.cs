namespace ReactNews.Infrastructure.Options.AdminSeed;

/// <summary>
/// What: Configuration values used to create a development/admin seed account.
/// How: The API binds the AdminSeed configuration section to this options type.
/// Why: Public registration should not create Admin users, but the application
/// still needs a controlled way to create the first admin account.
/// </summary>
public sealed class AdminSeedOptions
{
    public const string SectionName = "AdminSeed";

    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}
