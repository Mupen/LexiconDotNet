using ReactNews.Application.Contracts.Auth;
using ReactNews.Application.Interfaces;
using ReactNews.Application.Queries.Auth;
using ReactNews.Application.Services.Auth;
using ReactNews.Domain.Entities.Users;
using ReactNews.Domain.Enums.Users;

namespace ReactNews.UnitTests.Application;

/// <summary>
/// What: Tests the application-layer account use cases without HTTP cookies or EF Core.
/// How: Uses an in-memory IUserStore fake and calls RegisterUser, LoginUser, and GetCurrentUser directly.
/// Why: Authentication is a core feature, so the business rules should be proven separately from controller integration tests.
/// </summary>
public sealed class AuthUseCaseTests
{
    /// <summary>
    /// What: Checks that a valid registration creates a reader account.
    /// How: Executes RegisterUser with mixed-case email input and inspects the saved fake-store user.
    /// Why: Registration should normalize email, hash the password, and default normal readers without depending on HTTP.
    /// </summary>
    [Fact]
    public void RegisterUser_CreatesReaderAccount_WhenRequestIsValid()
    {
        var store = new FakeUserStore();
        var useCase = new RegisterUser(store);

        var result = useCase.Execute(new RegisterRequest(
            Email: "  Reader@Example.COM ",
            DisplayName: "Reader User",
            Password: "Password123!",
            Role: "Reader"));
        var savedUser = store.FindByEmail("reader@example.com");

        Assert.True(result.IsSuccess);
        Assert.NotNull(savedUser);
        Assert.Equal("reader@example.com", result.Value!.User.Email);
        Assert.Equal("Reader", result.Value.User.Role);
        Assert.NotEqual("Password123!", savedUser.PasswordHash);
        Assert.True(PasswordHasher.Verify("Password123!", savedUser.PasswordHash));
    }

    /// <summary>
    /// What: Checks that duplicate emails are rejected.
    /// How: Registers one user, then attempts to register the same normalized email again.
    /// Why: Account identity should be unique by email so login and profile lookup remain deterministic.
    /// </summary>
    [Fact]
    public void RegisterUser_ReturnsValidationFailure_WhenEmailAlreadyExists()
    {
        var store = new FakeUserStore();
        var useCase = new RegisterUser(store);

        useCase.Execute(new RegisterRequest("reader@example.com", "Reader User", "Password123!", "Reader"));
        var result = useCase.Execute(new RegisterRequest("READER@example.com", "Second User", "Password123!", "Reader"));

        Assert.False(result.IsSuccess);
        Assert.Equal("Email is already registered.", result.Error!.Message);
    }

    /// <summary>
    /// What: Checks that public registration cannot create an Admin account.
    /// How: Sends Admin in the legacy role field and verifies the saved user is still Reader.
    /// Why: Admin accounts should come from controlled seed/configuration, not public user-submitted role text.
    /// </summary>
    [Fact]
    public void RegisterUser_CreatesReader_WhenAdminRoleIsRequested()
    {
        var store = new FakeUserStore();
        var useCase = new RegisterUser(store);

        var result = useCase.Execute(new RegisterRequest("reader@example.com", "Reader User", "Password123!", "Admin"));

        Assert.True(result.IsSuccess);
        Assert.Equal("Reader", result.Value!.User.Role);
    }

    /// <summary>
    /// What: Checks that a valid login returns the saved user.
    /// How: Saves a fake user with a real password hash, then executes LoginUser with the matching password.
    /// Why: Login should depend on password verification, not on comparing plain text values.
    /// </summary>
    [Fact]
    public void LoginUser_ReturnsUser_WhenPasswordMatches()
    {
        var store = new FakeUserStore();
        var user = store.Save(CreateUser("reader@example.com", "Password123!"));
        var useCase = new LoginUser(store);

        var result = useCase.Execute(new LoginRequest("READER@example.com", "Password123!"));

        Assert.True(result.IsSuccess);
        Assert.Equal(user.Id, result.Value!.User.Id);
        Assert.Equal("reader@example.com", result.Value.User.Email);
    }

    /// <summary>
    /// What: Checks that a wrong password fails login.
    /// How: Saves a fake user and logs in with a different password.
    /// Why: Failed login must not reveal whether the email or password part was wrong.
    /// </summary>
    [Fact]
    public void LoginUser_ReturnsValidationFailure_WhenPasswordIsWrong()
    {
        var store = new FakeUserStore();
        store.Save(CreateUser("reader@example.com", "Password123!"));
        var useCase = new LoginUser(store);

        var result = useCase.Execute(new LoginRequest("reader@example.com", "WrongPassword123!"));

        Assert.False(result.IsSuccess);
        Assert.Equal("Email or password is incorrect.", result.Error!.Message);
    }

    /// <summary>
    /// What: Checks current-user lookup for an existing account.
    /// How: Saves a fake admin user and asks GetCurrentUser for the saved id.
    /// Why: The API cookie stores only a user id, so Application must turn that id back into a safe auth response.
    /// </summary>
    [Fact]
    public void GetCurrentUser_ReturnsUser_WhenIdExists()
    {
        var store = new FakeUserStore();
        var user = store.Save(CreateUser("admin@example.com", "Password123!", UserRole.Admin));
        var useCase = new GetCurrentUser(store);

        var result = useCase.Execute(user.Id);

        Assert.NotNull(result);
        Assert.Equal("Admin", result.User.Role);
    }

    /// <summary>
    /// What: Checks current-user lookup for a missing account.
    /// How: Executes GetCurrentUser with an id that the fake store does not know.
    /// Why: Deleted or invalid cookie identities should map to no current user instead of a fake account.
    /// </summary>
    [Fact]
    public void GetCurrentUser_ReturnsNull_WhenIdIsMissing()
    {
        var store = new FakeUserStore();
        var useCase = new GetCurrentUser(store);

        var result = useCase.Execute("missing-user-id");

        Assert.Null(result);
    }

    [Fact]
    public void UpdateProfile_ChangesDisplayName()
    {
        var store = new FakeUserStore();
        var user = store.Save(CreateUser("reader@example.com", "Password123!"));
        var useCase = new UpdateProfile(store);

        var result = useCase.Execute(user.Id, new UpdateProfileRequest("Updated Reader"));

        Assert.True(result.IsSuccess);
        Assert.Equal("Updated Reader", result.Value!.User.DisplayName);
        Assert.Equal("Updated Reader", store.FindById(user.Id)!.DisplayName);
    }

    /// <summary>
    /// What: Verifies that a correct current password allows a password change.
    /// How: saves a fake user, executes ChangePassword, and checks the stored hash
    /// verifies the new password but not the old one.
    /// Why: password changes must replace credentials rather than merely returning
    /// a successful response.
    /// </summary>
    [Fact]
    public void ChangePassword_ReplacesPasswordHash_WhenCurrentPasswordMatches()
    {
        var store = new FakeUserStore();
        var user = store.Save(CreateUser("reader@example.com", "Password123!"));
        var useCase = new ChangePassword(store);

        var result = useCase.Execute(user.Id, new ChangePasswordRequest("Password123!", "NewPassword123!"));
        var savedUser = store.FindById(user.Id)!;

        Assert.True(result.IsSuccess);
        Assert.True(PasswordHasher.Verify("NewPassword123!", savedUser.PasswordHash));
        Assert.False(PasswordHasher.Verify("Password123!", savedUser.PasswordHash));
    }

    /// <summary>
    /// What: Verifies that an incorrect current password is rejected.
    /// How: executes ChangePassword with a wrong current password and inspects the
    /// validation error.
    /// Why: account security depends on proving the caller knows the existing
    /// password before storing a new one.
    /// </summary>
    [Fact]
    public void ChangePassword_ReturnsValidationFailure_WhenCurrentPasswordIsWrong()
    {
        var store = new FakeUserStore();
        var user = store.Save(CreateUser("reader@example.com", "Password123!"));
        var useCase = new ChangePassword(store);

        var result = useCase.Execute(user.Id, new ChangePasswordRequest("WrongPassword123!", "NewPassword123!"));

        Assert.False(result.IsSuccess);
        Assert.Equal("Current password is incorrect.", result.Error!.Message);
    }

    /// <summary>
    /// What: Verifies that account deletion removes the user when the password matches.
    /// How: saves a fake user, executes DeleteAccount, and confirms id lookup
    /// returns null afterward.
    /// Why: deletion should be a real persistence change, not only a successful
    /// command result.
    /// </summary>
    [Fact]
    public void DeleteAccount_RemovesUser_WhenCurrentPasswordMatches()
    {
        var store = new FakeUserStore();
        var user = store.Save(CreateUser("reader@example.com", "Password123!"));
        var useCase = new DeleteAccount(store);

        var result = useCase.Execute(user.Id, new DeleteAccountRequest("Password123!"));

        Assert.True(result.IsSuccess);
        Assert.Null(store.FindById(user.Id));
    }

    /// <summary>
    /// What: Creates a valid user domain record for auth tests.
    /// How: assigns a generated id, supplied email/role, and a hashed supplied password.
    /// Why: test setup should reuse the same account shape as production use cases
    /// expect from persistence.
    /// </summary>
    private static User CreateUser(string email, string password, UserRole role = UserRole.Reader)
    {
        return new User(
            Id: Guid.NewGuid().ToString("N"),
            Email: email,
            DisplayName: "Test User",
            Role: role,
            PasswordHash: PasswordHasher.Hash(password),
            CreatedAtUtc: DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// What: In-memory user store used by auth use-case tests.
    /// How: Keeps users in dictionaries keyed by id and normalized email.
    /// Why: Application tests should verify auth rules without creating SQLite databases.
    /// </summary>
    private sealed class FakeUserStore : IUserStore
    {
        private readonly Dictionary<string, User> _usersById = new();
        private readonly Dictionary<string, User> _usersByEmail = new(StringComparer.OrdinalIgnoreCase);

        public User? FindByEmail(string email)
        {
            return _usersByEmail.GetValueOrDefault(email);
        }

        public User? FindById(string id)
        {
            return _usersById.GetValueOrDefault(id);
        }

        public User Save(User user)
        {
            _usersById[user.Id] = user;
            _usersByEmail[user.Email] = user;
            return user;
        }

        /// <summary>
        /// What: Removes one fake user from the in-memory store.
        /// How: finds by id, then removes both the id and email dictionary entries.
        /// Why: delete tests need future lookups and logins to behave as if the
        /// user row was removed from persistence.
        /// </summary>
        public bool Delete(string id)
        {
            var user = FindById(id);

            if (user is null)
            {
                return false;
            }

            _usersById.Remove(id);
            _usersByEmail.Remove(user.Email);
            return true;
        }
    }
}
