using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ReactNews.Application.Interfaces;
using ReactNews.Application.Services.Auth;
using ReactNews.Domain.Entities.Users;
using ReactNews.Domain.Enums.Users;
using ReactNews.Infrastructure.Options.AdminSeed;

namespace ReactNews.Infrastructure.Identity;

/// <summary>
/// What: Creates a configured Admin account when one is provided through configuration.
/// How: Reads AdminSeed options, checks the user store by email, hashes the
/// configured password, and saves an Admin user only when the email is not used.
/// Why: Admin creation should be controlled by developer/operator configuration,
/// not by the public registration page.
/// </summary>
public static class AdminSeeder
{
    /// <summary>
    /// What: Ensures the configured Admin account exists in persistence.
    /// How: creates a DI scope, reads AdminSeed options, validates the configured
    /// password length, checks for an existing email, and saves a new Admin user
    /// when the account is missing.
    /// Why: admin access should be repeatable across application starts without
    /// exposing an unauthenticated "create admin" endpoint.
    /// </summary>
    public static void EnsureSeedAdminCreated(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<AdminSeedOptions>>().Value;

        if (!HasSeedConfiguration(options))
        {
            return;
        }

        if (options.Password.Trim().Length < 8)
        {
            throw new InvalidOperationException("AdminSeed:Password must be at least 8 characters.");
        }

        var userStore = scope.ServiceProvider.GetRequiredService<IUserStore>();
        var email = options.Email.Trim().ToLowerInvariant();

        if (userStore.FindByEmail(email) is not null)
        {
            return;
        }

        var user = new User(
            Id: Guid.NewGuid().ToString("N"),
            Email: email,
            DisplayName: options.DisplayName.Trim(),
            Role: UserRole.Admin,
            PasswordHash: PasswordHasher.Hash(options.Password.Trim()),
            CreatedAtUtc: DateTimeOffset.UtcNow);

        userStore.Save(user);
    }

    /// <summary>
    /// What: Determines whether enough seed settings are present to create an Admin.
    /// How: trims email, display name, and password, then checks the minimum shape
    /// required before account creation can be attempted.
    /// Why: incomplete configuration should simply skip seeding, while malformed
    /// complete configuration can still fail with a clear runtime error.
    /// </summary>
    private static bool HasSeedConfiguration(AdminSeedOptions options)
    {
        var email = options.Email.Trim();
        var displayName = options.DisplayName.Trim();
        var password = options.Password.Trim();

        return email.Length >= 5
            && email.Contains('@')
            && displayName.Length >= 2
            && password.Length > 0;
    }
}
