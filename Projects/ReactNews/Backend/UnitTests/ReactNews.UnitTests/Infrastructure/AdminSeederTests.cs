using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ReactNews.Application.Interfaces;
using ReactNews.Application.Services.Auth;
using ReactNews.Domain.Entities.Users;
using ReactNews.Domain.Enums.Users;
using ReactNews.Infrastructure.Identity;
using ReactNews.Infrastructure.Options.AdminSeed;

namespace ReactNews.UnitTests.Infrastructure;

/// <summary>
/// What: Tests the controlled Admin seed flow.
/// How: Builds a tiny service provider with AdminSeedOptions and an in-memory IUserStore.
/// Why: Admin accounts should be created from configuration, not public registration.
/// </summary>
public sealed class AdminSeederTests
{
    [Fact]
    public void EnsureSeedAdminCreated_CreatesAdmin_WhenConfigurationIsComplete()
    {
        var userStore = new FakeUserStore();
        using var serviceProvider = CreateServiceProvider(userStore, new AdminSeedOptions
        {
            Email = "admin@example.com",
            DisplayName = "Seed Admin",
            Password = "Password123!"
        });

        serviceProvider.EnsureSeedAdminCreated();
        var admin = userStore.FindByEmail("admin@example.com");

        Assert.NotNull(admin);
        Assert.Equal(UserRole.Admin, admin.Role);
        Assert.True(PasswordHasher.Verify("Password123!", admin.PasswordHash));
    }

    /// <summary>
    /// What: Verifies that missing Admin seed configuration does not create users.
    /// How: builds the seed service provider with default empty options and checks
    /// the fake store after seeding.
    /// Why: optional seed configuration should be safe to omit in environments that
    /// do not need an initial admin account.
    /// </summary>
    [Fact]
    public void EnsureSeedAdminCreated_DoesNothing_WhenConfigurationIsMissing()
    {
        var userStore = new FakeUserStore();
        using var serviceProvider = CreateServiceProvider(userStore, new AdminSeedOptions());

        serviceProvider.EnsureSeedAdminCreated();

        Assert.Empty(userStore.Users);
    }

    /// <summary>
    /// What: Builds the minimal dependency injection container required by AdminSeeder.
    /// How: registers the fake user store and AdminSeedOptions as services.
    /// Why: the test should execute the real extension method without booting the
    /// full API host.
    /// </summary>
    private static ServiceProvider CreateServiceProvider(
        FakeUserStore userStore,
        AdminSeedOptions options)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IUserStore>(userStore);
        services.AddSingleton(Options.Create(options));
        return services.BuildServiceProvider();
    }

    private sealed class FakeUserStore : IUserStore
    {
        private readonly Dictionary<string, User> _usersByEmail = new(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyCollection<User> Users => _usersByEmail.Values;

        /// <summary>
        /// What: Finds a fake user by email.
        /// How: reads from a case-insensitive dictionary keyed by email.
        /// Why: AdminSeeder checks email existence before creating a seed account.
        /// </summary>
        public User? FindByEmail(string email)
        {
            return _usersByEmail.GetValueOrDefault(email);
        }

        /// <summary>
        /// What: Finds a fake user by id.
        /// How: searches the in-memory user values for the matching id.
        /// Why: the fake implements the full IUserStore interface even though these
        /// tests focus mainly on email lookup.
        /// </summary>
        public User? FindById(string id)
        {
            return _usersByEmail.Values.SingleOrDefault(user => user.Id == id);
        }

        /// <summary>
        /// What: Saves a fake user.
        /// How: writes the user into the email dictionary.
        /// Why: AdminSeeder must be able to persist the newly seeded account.
        /// </summary>
        public User Save(User user)
        {
            _usersByEmail[user.Email] = user;
            return user;
        }

        /// <summary>
        /// What: Deletes a fake user by id.
        /// How: finds the user and removes the matching email dictionary entry.
        /// Why: the fake must satisfy IUserStore even when deletion is not the
        /// primary behavior under test.
        /// </summary>
        public bool Delete(string id)
        {
            var user = FindById(id);

            if (user is null)
            {
                return false;
            }

            _usersByEmail.Remove(user.Email);
            return true;
        }
    }
}
