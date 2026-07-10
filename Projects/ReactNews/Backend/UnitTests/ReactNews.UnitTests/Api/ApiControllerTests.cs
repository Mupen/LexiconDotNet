using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ReactNews.Application.Interfaces;
using ReactNews.Application.Models.Articles;
using ReactNews.Application.Models.Sources;
using ReactNews.Application.Services.Auth;
using ReactNews.Domain.Entities.Articles;
using ReactNews.Domain.Entities.EditorialArticles;
using ReactNews.Domain.Entities.ReaderPreferences;
using ReactNews.Domain.Entities.SavedArticles;
using ReactNews.Domain.Entities.Sources;
using ReactNews.Domain.Entities.Users;
using ReactNews.Domain.Enums.EditorialArticles;
using ReactNews.Domain.Enums.Users;

namespace ReactNews.UnitTests.Api;

/// <summary>
/// What: Tests the real ReactNews HTTP API surface in memory.
/// How: WebApplicationFactory boots the actual API pipeline, while ConfigureTestServices replaces external news/storage dependencies with fakes.
/// Why: Unit tests prove individual classes, but integration tests prove routing, dependency injection, model binding, result mapping, and JSON output work together.
/// </summary>
public sealed class ApiControllerTests
{
    /// <summary>
    /// What: Checks that the health endpoint answers successfully.
    /// How: Creates an in-memory API client and sends GET /api/health.
    /// Why: Health is the simplest endpoint and should confirm the API can start without calling NewsAPI or the database.
    /// </summary>
    [Fact]
    public async Task Health_ReturnsOk()
    {
        using var factory = new ReactNewsApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/health");
        using var json = await ReadJsonAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("ok", json.RootElement.GetProperty("status").GetString());
        Assert.Equal("ReactNews.Api", json.RootElement.GetProperty("application").GetString());
    }

    /// <summary>
    /// What: Checks that /api/articles returns a normalized article-list response.
    /// How: Calls the real route with headline query parameters and reads the returned JSON.
    /// Why: This proves controller binding, ArticleListRequest creation, GetArticles execution, and response serialization work together.
    /// </summary>
    [Fact]
    public async Task Articles_ReturnsArticleList()
    {
        using var factory = new ReactNewsApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/articles?mode=headlines&country=us&category=technology&page=1&pageSize=20");
        using var json = await ReadJsonAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("headlines", json.RootElement.GetProperty("mode").GetString());
        Assert.Equal(1, json.RootElement.GetProperty("items").GetArrayLength());
        Assert.Equal("integration-article", json.RootElement.GetProperty("items")[0].GetProperty("id").GetString());
    }

    /// <summary>
    /// What: Checks that invalid article requests become HTTP 400 validation responses.
    /// How: Calls /api/articles with an unsupported mode and inspects the standard error contract.
    /// Why: Expected user/input problems should not become 500 errors, and the frontend needs a predictable error shape.
    /// </summary>
    [Fact]
    public async Task Articles_ReturnsBadRequest_WhenModeIsInvalid()
    {
        using var factory = new ReactNewsApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/articles?mode=invalid");
        using var json = await ReadJsonAsync(response);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("validation_error", json.RootElement.GetProperty("code").GetString());
        Assert.Equal("Mode must be either headlines or search.", json.RootElement.GetProperty("error").GetString());
    }

    /// <summary>
    /// What: Checks that /api/articles/{id} returns a stored article snapshot.
    /// How: The fake snapshot store is pre-seeded with integration-article and the test requests that id.
    /// Why: The detail route depends on snapshot storage, so the HTTP layer must correctly map an existing snapshot to 200 OK.
    /// </summary>
    [Fact]
    public async Task ArticleDetail_ReturnsArticle_WhenSnapshotExists()
    {
        using var factory = new ReactNewsApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/articles/integration-article");
        using var json = await ReadJsonAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("integration-article", json.RootElement.GetProperty("id").GetString());
        Assert.Equal("Integration test article", json.RootElement.GetProperty("title").GetString());
    }

    /// <summary>
    /// What: Checks that unknown article ids return HTTP 404.
    /// How: Requests an id that the fake snapshot store does not contain.
    /// Why: Missing snapshots are normal user-flow problems, so the API should return not-found instead of an exception.
    /// </summary>
    [Fact]
    public async Task ArticleDetail_ReturnsNotFound_WhenSnapshotIsMissing()
    {
        using var factory = new ReactNewsApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/articles/missing");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>
    /// What: Checks that /api/sources returns source metadata.
    /// How: Calls the real route with filter parameters and reads the source list JSON.
    /// Why: This proves the sources controller, SourceListRequest binding, GetSources use case, and source DTO serialization work together.
    /// </summary>
    [Fact]
    public async Task Sources_ReturnsSourceList()
    {
        using var factory = new ReactNewsApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/sources?category=technology&language=en&country=us");
        using var json = await ReadJsonAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, json.RootElement.GetProperty("items").GetArrayLength());
        Assert.Equal("integration-source", json.RootElement.GetProperty("items")[0].GetProperty("id").GetString());
    }

    /// <summary>
    /// What: Checks that saved articles can be listed through HTTP.
    /// How: Calls GET /api/saved-articles against the in-memory API host.
    /// Why: The frontend saved page depends on this route returning the standard Items wrapper.
    /// </summary>
    [Fact]
    public async Task SavedArticles_ReturnsSavedList()
    {
        using var factory = new ReactNewsApiFactory();
        using var client = factory.CreateClient();
        await LoginAsReader(client);

        await client.PostAsync("/api/saved-articles/integration-article", content: null);
        using var response = await client.GetAsync("/api/saved-articles");
        using var json = await ReadJsonAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, json.RootElement.GetProperty("items").GetArrayLength());
        Assert.Equal("integration-article", json.RootElement.GetProperty("items")[0].GetProperty("id").GetString());
    }

    /// <summary>
    /// What: Checks that an article can be saved by snapshot id.
    /// How: Calls POST /api/saved-articles/integration-article and reads the saved DTO response.
    /// Why: This proves the route, snapshot lookup, save use case, and result mapping work together.
    /// </summary>
    [Fact]
    public async Task SaveArticle_ReturnsSavedArticle_WhenSnapshotExists()
    {
        using var factory = new ReactNewsApiFactory();
        using var client = factory.CreateClient();
        await LoginAsReader(client);

        using var response = await client.PostAsync("/api/saved-articles/integration-article", content: null);
        using var json = await ReadJsonAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("integration-article", json.RootElement.GetProperty("id").GetString());
        Assert.True(json.RootElement.TryGetProperty("savedAtUtc", out _));
    }

    /// <summary>
    /// What: Checks that saving a missing snapshot returns HTTP 404.
    /// How: Calls POST with an id that the fake snapshot store does not contain.
    /// Why: A user can only save articles ReactNews has already loaded and remembered.
    /// </summary>
    [Fact]
    public async Task SaveArticle_ReturnsNotFound_WhenSnapshotIsMissing()
    {
        using var factory = new ReactNewsApiFactory();
        using var client = factory.CreateClient();
        await LoginAsReader(client);

        using var response = await client.PostAsync("/api/saved-articles/missing", content: null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>
    /// What: Checks that a saved article can be removed through HTTP DELETE.
    /// How: Saves the article, deletes it, then verifies the saved list is empty.
    /// Why: Save/remove should work as a complete reader workflow, not only separate isolated commands.
    /// </summary>
    [Fact]
    public async Task RemoveSavedArticle_DeletesSavedArticle()
    {
        using var factory = new ReactNewsApiFactory();
        using var client = factory.CreateClient();
        await LoginAsReader(client);

        await client.PostAsync("/api/saved-articles/integration-article", content: null);
        using var deleteResponse = await client.DeleteAsync("/api/saved-articles/integration-article");
        using var listResponse = await client.GetAsync("/api/saved-articles");
        using var json = await ReadJsonAsync(listResponse);

        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        Assert.Equal(0, json.RootElement.GetProperty("items").GetArrayLength());
    }

    /// <summary>
    /// What: Checks that reader preferences can be loaded through HTTP.
    /// How: Calls GET /api/reader-preferences and inspects the serialized preference object.
    /// Why: The frontend starts from this route to apply theme, font, and category settings.
    /// </summary>
    [Fact]
    public async Task ReaderPreferences_ReturnsPreferences()
    {
        using var factory = new ReactNewsApiFactory();
        using var client = factory.CreateClient();
        await LoginAsReader(client);

        using var response = await client.GetAsync("/api/reader-preferences");
        using var json = await ReadJsonAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("light", json.RootElement.GetProperty("theme").GetString());
        Assert.True(json.RootElement.GetProperty("preferredCategories").GetArrayLength() > 0);
    }

    /// <summary>
    /// What: Checks that reader preferences can be updated through HTTP.
    /// How: Sends PUT JSON and verifies the saved response values.
    /// Why: This proves model binding, validation, use case execution, and JSON serialization for preferences.
    /// </summary>
    [Fact]
    public async Task UpdateReaderPreferences_SavesPreferences()
    {
        using var factory = new ReactNewsApiFactory();
        using var client = factory.CreateClient();
        await LoginAsReader(client);

        using var response = await client.PutAsJsonAsync("/api/reader-preferences", new
        {
            theme = "dark",
            fontScale = 1.15m,
            compactCards = true,
            preferredCategories = new[] { "sports", "technology" }
        });
        using var json = await ReadJsonAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("dark", json.RootElement.GetProperty("theme").GetString());
        Assert.True(json.RootElement.GetProperty("compactCards").GetBoolean());
    }

    /// <summary>
    /// What: Checks that invalid preference updates return HTTP 400.
    /// How: Sends an unsupported theme value.
    /// Why: The API should protect persisted preference data from bad manual requests.
    /// </summary>
    [Fact]
    public async Task UpdateReaderPreferences_ReturnsBadRequest_WhenInvalid()
    {
        using var factory = new ReactNewsApiFactory();
        using var client = factory.CreateClient();
        await LoginAsReader(client);

        using var response = await client.PutAsJsonAsync("/api/reader-preferences", new
        {
            theme = "blue",
            fontScale = 1.0m,
            compactCards = false,
            preferredCategories = new[] { "technology" }
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// What: Checks that two signed-in readers do not share saved articles or preferences.
    /// How: Uses two HttpClient instances with separate auth cookies against the same in-memory API host.
    /// Why: Account login should mean user-owned data, not just access to one shared local saved/preference profile.
    /// </summary>
    [Fact]
    public async Task ReaderData_IsIsolatedBetweenUsers()
    {
        using var factory = new ReactNewsApiFactory();
        using var firstReader = factory.CreateClient();
        using var secondReader = factory.CreateClient();
        await LoginAsReader(firstReader);
        await LoginAsReader(secondReader);

        await firstReader.PostAsync("/api/saved-articles/integration-article", content: null);
        await firstReader.PutAsJsonAsync("/api/reader-preferences", new
        {
            theme = "dark",
            fontScale = 1.1m,
            compactCards = true,
            preferredCategories = new[] { "sports" }
        });
        using var firstSavedResponse = await firstReader.GetAsync("/api/saved-articles");
        using var firstSavedJson = await ReadJsonAsync(firstSavedResponse);
        using var secondSavedResponse = await secondReader.GetAsync("/api/saved-articles");
        using var secondSavedJson = await ReadJsonAsync(secondSavedResponse);
        using var secondPreferencesResponse = await secondReader.GetAsync("/api/reader-preferences");
        using var secondPreferencesJson = await ReadJsonAsync(secondPreferencesResponse);

        Assert.Equal(1, firstSavedJson.RootElement.GetProperty("items").GetArrayLength());
        Assert.Equal(0, secondSavedJson.RootElement.GetProperty("items").GetArrayLength());
        Assert.Equal("light", secondPreferencesJson.RootElement.GetProperty("theme").GetString());
    }

    /// <summary>
    /// What: Verifies that an admin can create, list, and publish editorial content through HTTP.
    /// How: logs in as the seeded admin, posts a draft, reads the admin list, and
    /// calls the publish command endpoint.
    /// Why: this protects the complete admin editorial flow from route, auth, and
    /// result-mapping regressions.
    /// </summary>
    [Fact]
    public async Task EditorialArticles_CanBeCreatedListedAndPublished()
    {
        using var factory = new ReactNewsApiFactory();
        using var client = factory.CreateClient();
        await LoginAsAdmin(client);

        using var createResponse = await client.PostAsJsonAsync("/api/editorial/articles", CreateEditorialRequest());
        using var createdJson = await ReadJsonAsync(createResponse);
        var id = createdJson.RootElement.GetProperty("id").GetString();

        using var listResponse = await client.GetAsync("/api/editorial/articles");
        using var listJson = await ReadJsonAsync(listResponse);
        using var publishResponse = await client.PostAsync($"/api/editorial/articles/{id}/publish", content: null);
        using var publishedJson = await ReadJsonAsync(publishResponse);

        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        Assert.Equal(1, listJson.RootElement.GetProperty("items").GetArrayLength());
        Assert.Equal("Published", publishedJson.RootElement.GetProperty("status").GetString());
    }

    /// <summary>
    /// What: Verifies that anonymous readers only see published editorial articles.
    /// How: creates one draft and one published article through the admin API, then
    /// reads the public feed and public detail route without authentication.
    /// Why: public editorial routes must never expose drafts or archived/private
    /// admin content.
    /// </summary>
    [Fact]
    public async Task PublicEditorialArticles_ReturnsOnlyPublishedArticlesForAnonymousReaders()
    {
        using var factory = new ReactNewsApiFactory();
        using var adminClient = factory.CreateClient();
        using var publicClient = factory.CreateClient();
        await LoginAsAdmin(adminClient);

        await adminClient.PostAsJsonAsync("/api/editorial/articles", CreateEditorialRequest(title: "Draft public-hidden article"));
        using var createResponse = await adminClient.PostAsJsonAsync("/api/editorial/articles", CreateEditorialRequest(title: "Published public article"));
        using var createdJson = await ReadJsonAsync(createResponse);
        var id = createdJson.RootElement.GetProperty("id").GetString();
        await adminClient.PostAsync($"/api/editorial/articles/{id}/publish", content: null);

        using var listResponse = await publicClient.GetAsync("/api/public/editorial/articles");
        using var listJson = await ReadJsonAsync(listResponse);
        using var detailResponse = await publicClient.GetAsync($"/api/public/editorial/articles/{id}");
        using var detailJson = await ReadJsonAsync(detailResponse);

        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        Assert.Equal(1, listJson.RootElement.GetProperty("items").GetArrayLength());
        Assert.Equal("Published public article", listJson.RootElement.GetProperty("items")[0].GetProperty("title").GetString());
        Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);
        Assert.Equal(id, detailJson.RootElement.GetProperty("id").GetString());
    }

    /// <summary>
    /// What: Verifies that the public detail endpoint hides draft articles.
    /// How: creates a draft through the admin API and requests it through the public
    /// detail route.
    /// Why: a known id should not bypass the publication workflow.
    /// </summary>
    [Fact]
    public async Task PublicEditorialArticleDetail_ReturnsNotFound_WhenArticleIsDraft()
    {
        using var factory = new ReactNewsApiFactory();
        using var adminClient = factory.CreateClient();
        using var publicClient = factory.CreateClient();
        await LoginAsAdmin(adminClient);

        using var createResponse = await adminClient.PostAsJsonAsync("/api/editorial/articles", CreateEditorialRequest());
        using var createdJson = await ReadJsonAsync(createResponse);
        var id = createdJson.RootElement.GetProperty("id").GetString();

        using var response = await publicClient.GetAsync($"/api/public/editorial/articles/{id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>
    /// What: Verifies that invalid editorial creation requests return Bad Request.
    /// How: posts a request with a title that is shorter than the application rule.
    /// Why: validation failures should be represented as expected client errors and
    /// should not create stored articles.
    /// </summary>
    [Fact]
    public async Task EditorialArticles_ReturnsBadRequest_WhenCreateRequestIsInvalid()
    {
        using var factory = new ReactNewsApiFactory();
        using var client = factory.CreateClient();
        await LoginAsAdmin(client);

        using var response = await client.PostAsJsonAsync("/api/editorial/articles", CreateEditorialRequest(title: "Bad"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// What: Checks that removed compatibility routes stay removed.
    /// How: Calls the old /api/news/search route and expects HTTP 404.
    /// Why: This protects the project decision that ReactNews should have one public article API shape.
    /// </summary>
    [Fact]
    public async Task RemovedNewsSearchRoute_ReturnsNotFound()
    {
        using var factory = new ReactNewsApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/news/search?q=react");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>
    /// What: Reads an HTTP response body as JsonDocument.
    /// How: Ensures the response content stream is parsed through System.Text.Json.
    /// Why: Integration tests should inspect the real serialized JSON instead of calling controllers directly.
    /// </summary>
    private static async Task<JsonDocument> ReadJsonAsync(HttpResponseMessage response)
    {
        return await response.Content.ReadFromJsonAsync<JsonDocument>()
            ?? throw new InvalidOperationException("The API response did not contain valid JSON.");
    }

    /// <summary>
    /// What: Customizes the real API host for integration tests.
    /// How: WebApplicationFactory starts Program, then ConfigureTestServices removes real provider/storage services and registers fakes.
    /// Why: The tests should exercise the real HTTP pipeline without calling NewsAPI or depending on a real article snapshot database.
    /// </summary>
    private sealed class ReactNewsApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<INewsProvider>();
                services.RemoveAll<IEditorialArticleStore>();
                services.RemoveAll<IArticleSnapshotStore>();
                services.RemoveAll<IReaderPreferencesStore>();
                services.RemoveAll<ISavedArticleStore>();
                services.RemoveAll<IUserStore>();

                services.AddSingleton<INewsProvider, FakeNewsProvider>();
                services.AddSingleton<IEditorialArticleStore, FakeEditorialArticleStore>();
                services.AddSingleton<IArticleSnapshotStore, FakeArticleSnapshotStore>();
                services.AddSingleton<IReaderPreferencesStore, FakeReaderPreferencesStore>();
                services.AddSingleton<ISavedArticleStore, FakeSavedArticleStore>();
                services.AddSingleton<FakeUserStore>();
                services.AddSingleton<IUserStore>(serviceProvider => serviceProvider.GetRequiredService<FakeUserStore>());
            });
        }
    }

    /// <summary>
    /// What: Verifies register, cookie sign-in, and current-user lookup together.
    /// How: posts a registration request and then calls /api/auth/me using the same
    /// HttpClient so the auth cookie is retained.
    /// Why: the frontend depends on registration immediately creating a usable
    /// authenticated session.
    /// </summary>
    [Fact]
    public async Task Auth_RegisterLoginAndMe_ReturnsCurrentUser()
    {
        using var factory = new ReactNewsApiFactory();
        using var client = factory.CreateClient();

        using var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "new-reader@example.com",
            displayName = "New Reader",
            password = "Password123!",
            role = "Reader"
        });
        using var meResponse = await client.GetAsync("/api/auth/me");
        using var json = await ReadJsonAsync(meResponse);

        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);
        Assert.Equal("new-reader@example.com", json.RootElement.GetProperty("user").GetProperty("email").GetString());
    }

    /// <summary>
    /// What: Verifies that public registration cannot create Admin accounts.
    /// How: sends a registration payload that asks for Admin and checks the stored
    /// current user role.
    /// Why: admin access must come from seed/configuration, not from a public role
    /// field supplied by the browser.
    /// </summary>
    [Fact]
    public async Task Auth_RegisterCreatesReader_WhenAdminRoleIsRequested()
    {
        using var factory = new ReactNewsApiFactory();
        using var client = factory.CreateClient();

        using var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "public-admin-request@example.com",
            displayName = "Public Admin Request",
            password = "Password123!",
            role = "Admin"
        });
        using var meResponse = await client.GetAsync("/api/auth/me");
        using var json = await ReadJsonAsync(meResponse);

        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);
        Assert.Equal("Reader", json.RootElement.GetProperty("user").GetProperty("role").GetString());
    }

    /// <summary>
    /// What: Checks that private reader/admin endpoints reject anonymous visitors.
    /// How: Calls saved articles, reader preferences, and editorial routes without registering or logging in.
    /// Why: These features contain personal or admin-only data, so a missing auth cookie must stop the request before use cases run.
    /// </summary>
    [Fact]
    public async Task PrivateEndpoints_ReturnUnauthorized_WhenAnonymous()
    {
        using var factory = new ReactNewsApiFactory();
        using var client = factory.CreateClient();

        using var savedResponse = await client.GetAsync("/api/saved-articles");
        using var preferencesResponse = await client.GetAsync("/api/reader-preferences");
        using var editorialResponse = await client.GetAsync("/api/editorial/articles");

        Assert.Equal(HttpStatusCode.Unauthorized, savedResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, preferencesResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, editorialResponse.StatusCode);
    }

    /// <summary>
    /// What: Checks that a duplicate registration is rejected by the HTTP API.
    /// How: Registers one account, logs out, then posts a second registration with the same email.
    /// Why: The controller should expose the application validation rule as HTTP 400 instead of creating two identities.
    /// </summary>
    [Fact]
    public async Task Auth_RegisterReturnsBadRequest_WhenEmailAlreadyExists()
    {
        using var factory = new ReactNewsApiFactory();
        using var client = factory.CreateClient();

        await client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "duplicate@example.com",
            displayName = "First Reader",
            password = "Password123!",
            role = "Reader"
        });
        await client.PostAsync("/api/auth/logout", content: null);
        using var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "DUPLICATE@example.com",
            displayName = "Second Reader",
            password = "Password123!",
            role = "Reader"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// What: Checks that login rejects an incorrect password.
    /// How: Registers a reader, logs out, then attempts login with the right email and wrong password.
    /// Why: Failed login should return a validation failure and should not create a new auth session.
    /// </summary>
    [Fact]
    public async Task Auth_LoginReturnsBadRequest_WhenPasswordIsWrong()
    {
        using var factory = new ReactNewsApiFactory();
        using var client = factory.CreateClient();

        await client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "login-failure@example.com",
            displayName = "Login Failure",
            password = "Password123!",
            role = "Reader"
        });
        await client.PostAsync("/api/auth/logout", content: null);
        using var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "login-failure@example.com",
            password = "WrongPassword123!"
        });
        using var meResponse = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, meResponse.StatusCode);
    }

    /// <summary>
    /// What: Checks that logout removes the current auth cookie.
    /// How: Registers a reader, verifies /api/auth/me works, posts logout, then verifies /api/auth/me is unauthorized.
    /// Why: Users need a reliable sign-out flow before shared computers or role changes can be handled safely.
    /// </summary>
    [Fact]
    public async Task Auth_LogoutClearsCurrentUserSession()
    {
        using var factory = new ReactNewsApiFactory();
        using var client = factory.CreateClient();

        await LoginAsReader(client);
        using var authenticatedMeResponse = await client.GetAsync("/api/auth/me");
        using var logoutResponse = await client.PostAsync("/api/auth/logout", content: null);
        using var anonymousMeResponse = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.OK, authenticatedMeResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, anonymousMeResponse.StatusCode);
    }

    /// <summary>
    /// What: Verifies that profile updates change the current user data.
    /// How: logs in as a reader, updates the display name, and then reads
    /// /api/auth/me to confirm the refreshed cookie/session data.
    /// Why: profile edits must be visible immediately after the request succeeds.
    /// </summary>
    [Fact]
    public async Task Auth_UpdateProfileChangesCurrentUser()
    {
        using var factory = new ReactNewsApiFactory();
        using var client = factory.CreateClient();
        await LoginAsReader(client);

        using var response = await client.PutAsJsonAsync("/api/auth/profile", new
        {
            displayName = "Updated Reader"
        });
        using var json = await ReadJsonAsync(response);
        using var meResponse = await client.GetAsync("/api/auth/me");
        using var meJson = await ReadJsonAsync(meResponse);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Updated Reader", json.RootElement.GetProperty("user").GetProperty("displayName").GetString());
        Assert.Equal("Updated Reader", meJson.RootElement.GetProperty("user").GetProperty("displayName").GetString());
    }

    /// <summary>
    /// What: Verifies that password changes replace the login credential.
    /// How: registers a reader, changes the password, logs out, and logs back in
    /// with the new password.
    /// Why: the account page must support a real password-change workflow rather
    /// than only updating stored profile fields.
    /// </summary>
    [Fact]
    public async Task Auth_ChangePasswordAllowsLoginWithNewPassword()
    {
        using var factory = new ReactNewsApiFactory();
        using var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "password-change@example.com",
            displayName = "Password Change",
            password = "Password123!",
            role = "Reader"
        });

        using var changeResponse = await client.PutAsJsonAsync("/api/auth/password", new
        {
            currentPassword = "Password123!",
            newPassword = "NewPassword123!"
        });
        await client.PostAsync("/api/auth/logout", content: null);
        using var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "password-change@example.com",
            password = "NewPassword123!"
        });

        Assert.Equal(HttpStatusCode.OK, changeResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }

    /// <summary>
    /// What: Verifies that account deletion signs out and removes the account.
    /// How: registers a reader, deletes the account with the current password, then
    /// checks that /me is unauthorized and old credentials no longer log in.
    /// Why: deleted accounts must not leave an active session or reusable login.
    /// </summary>
    [Fact]
    public async Task Auth_DeleteAccountSignsOutAndRemovesUser()
    {
        using var factory = new ReactNewsApiFactory();
        using var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "delete-account@example.com",
            displayName = "Delete Account",
            password = "Password123!",
            role = "Reader"
        });

        using var deleteResponse = await client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/api/auth/account")
        {
            Content = JsonContent.Create(new { currentPassword = "Password123!" })
        });
        using var meResponse = await client.GetAsync("/api/auth/me");
        using var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "delete-account@example.com",
            password = "Password123!"
        });

        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, meResponse.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, loginResponse.StatusCode);
    }

    /// <summary>
    /// What: Verifies that reader accounts cannot use admin editorial endpoints.
    /// How: logs in as a normal reader and requests the admin editorial list.
    /// Why: role-based authorization should block authenticated users who do not
    /// have the Admin role.
    /// </summary>
    [Fact]
    public async Task EditorialArticles_ReturnsForbidden_ForReader()
    {
        using var factory = new ReactNewsApiFactory();
        using var client = factory.CreateClient();
        await LoginAsReader(client);

        using var response = await client.GetAsync("/api/editorial/articles");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>
    /// What: Signs the supplied test client in as a unique reader.
    /// How: posts a registration request with a generated email address.
    /// Why: each test can start from an authenticated reader session without
    /// sharing account state with other tests.
    /// </summary>
    private static async Task LoginAsReader(HttpClient client)
    {
        await client.PostAsJsonAsync("/api/auth/register", new
        {
            email = $"reader-{Guid.NewGuid():N}@example.com",
            displayName = "Reader User",
            password = "Password123!",
            role = "Reader"
        });
    }

    /// <summary>
    /// What: Signs the supplied test client in as the seeded admin account.
    /// How: posts the known admin email/password configured in the fake user store.
    /// Why: admin-route tests need an authenticated Admin cookie but should not
    /// duplicate login request construction in every test.
    /// </summary>
    private static async Task LoginAsAdmin(HttpClient client)
    {
        await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "admin@example.com",
            password = "Password123!"
        });
    }

    /// <summary>
    /// What: Builds a valid editorial article request object for HTTP tests.
    /// How: returns an anonymous object that matches the JSON contract expected by
    /// the controller.
    /// Why: tests can override only the field under test while keeping the rest of
    /// the editorial request valid.
    /// </summary>
    private static object CreateEditorialRequest(string title = "Integration editorial article")
    {
        return new
        {
            title,
            summary = "This editorial article is created by an integration test.",
            body = "This is the body for an editorial integration test article and it is long enough for validation.",
            author = "Admin Editor",
            category = "technology",
            imageUrl = "",
            status = "Draft"
        };
    }

    private sealed class FakeEditorialArticleStore : IEditorialArticleStore
    {
        private readonly Dictionary<string, EditorialArticle> _articles = new();

        /// <summary>
        /// What: Lists fake editorial articles for API tests.
        /// How: returns in-memory values ordered by update time descending.
        /// Why: the fake should match the real store ordering contract closely
        /// enough for controller-level assertions.
        /// </summary>
        public IReadOnlyList<EditorialArticle> List()
        {
            return _articles.Values.OrderByDescending(article => article.UpdatedAtUtc).ToList();
        }

        /// <summary>
        /// What: Finds one fake editorial article by id.
        /// How: uses the dictionary key lookup and returns null for missing ids.
        /// Why: controller tests need the same null-to-404 behavior as production.
        /// </summary>
        public EditorialArticle? Find(string id)
        {
            return _articles.GetValueOrDefault(id);
        }

        /// <summary>
        /// What: Inserts or replaces one fake editorial article.
        /// How: stores the article in a dictionary keyed by article id.
        /// Why: admin create/update/publish tests need persisted state inside one
        /// test factory without using SQLite.
        /// </summary>
        public EditorialArticle Save(EditorialArticle article)
        {
            _articles[article.Id] = article;
            return article;
        }
    }

    private sealed class FakeUserStore : IUserStore
    {
        private readonly Dictionary<string, User> _usersById = new();
        private readonly Dictionary<string, User> _usersByEmail = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// What: Creates the in-memory account store with one Admin account.
        /// How: saves a user with the known admin email and a hashed test password.
        /// Why: admin integration tests need role authorization without relying on
        /// startup seed configuration or a database.
        /// </summary>
        public FakeUserStore()
        {
            Save(new User(
                Id: "seed-admin",
                Email: "admin@example.com",
                DisplayName: "Seed Admin",
                Role: UserRole.Admin,
                PasswordHash: PasswordHasher.Hash("Password123!"),
                CreatedAtUtc: DateTimeOffset.UtcNow));
        }

        /// <summary>
        /// What: Finds an in-memory user by email.
        /// How: reads from the case-insensitive email dictionary.
        /// Why: login and duplicate-registration tests need realistic email lookup.
        /// </summary>
        public User? FindByEmail(string email)
        {
            return _usersByEmail.GetValueOrDefault(email);
        }

        /// <summary>
        /// What: Finds an in-memory user by id.
        /// How: reads from the id dictionary and returns null when missing.
        /// Why: /api/auth/me and account-management tests need id-based lookup from
        /// the authentication claim.
        /// </summary>
        public User? FindById(string id)
        {
            return _usersById.GetValueOrDefault(id);
        }

        /// <summary>
        /// What: Saves an in-memory user.
        /// How: writes the same user to both id and email dictionaries.
        /// Why: the fake store must support all application lookup paths after a
        /// registration or profile/password update.
        /// </summary>
        public User Save(User user)
        {
            _usersById[user.Id] = user;
            _usersByEmail[user.Email] = user;
            return user;
        }

        /// <summary>
        /// What: Deletes an in-memory user.
        /// How: removes the user from both dictionaries when the id exists.
        /// Why: account deletion tests need the deleted account to disappear from
        /// both id lookup and future login by email.
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

    /// <summary>
    /// What: Fake reader preference store used by API integration tests.
    /// How: Keeps one preference object in memory for a test factory.
    /// Why: HTTP tests should prove preference routing and validation without depending on a SQLite file.
    /// </summary>
    private sealed class FakeReaderPreferencesStore : IReaderPreferencesStore
    {
        private readonly Dictionary<string, ReaderPreferences> _preferencesByUser = new();

        /// <summary>
        /// What: Gets fake reader preferences for one user.
        /// How: returns stored preferences when present, otherwise returns the same
        /// default values expected by the real store.
        /// Why: API tests should not require a separate setup call before checking
        /// default preference behavior.
        /// </summary>
        public ReaderPreferences Get(string userId)
        {
            if (_preferencesByUser.TryGetValue(userId, out var preferences))
            {
                return preferences;
            }

            return new ReaderPreferences(
                Theme: "light",
                FontScale: 1.0m,
                CompactCards: false,
                PreferredCategories: new[] { "technology", "general" });
        }

        /// <summary>
        /// What: Saves fake reader preferences for one user.
        /// How: stores the preference object in a dictionary keyed by user id.
        /// Why: preference update tests need user-specific state during the test.
        /// </summary>
        public ReaderPreferences Save(string userId, ReaderPreferences preferences)
        {
            _preferencesByUser[userId] = preferences;
            return preferences;
        }
    }

    /// <summary>
    /// What: Fake saved-article store used by API integration tests.
    /// How: Stores saved articles in memory for the lifetime of one WebApplicationFactory.
    /// Why: HTTP tests should prove API behavior without depending on SQLite file state.
    /// </summary>
    private sealed class FakeSavedArticleStore : ISavedArticleStore
    {
        private readonly Dictionary<string, SavedArticle> _articles = new();

        /// <summary>
        /// What: Saves or replaces one fake saved article.
        /// How: stores a SavedArticle by a composite key of user id and article id.
        /// Why: this mirrors the real per-user idempotent save behavior closely
        /// enough for HTTP tests.
        /// </summary>
        public SavedArticle Save(string userId, Article article, DateTimeOffset savedAtUtc)
        {
            // What: Saves or replaces the article in memory.
            // How: Stores a SavedArticle by user id and article id.
            // Why: This mirrors the real per-user idempotent save behavior closely enough for HTTP tests.
            var savedArticle = new SavedArticle(article, savedAtUtc);
            _articles[$"{userId}:{article.Id}"] = savedArticle;
            return savedArticle;
        }

        /// <summary>
        /// What: Lists fake saved articles for one user.
        /// How: filters dictionary entries by user id and sorts by saved time
        /// descending.
        /// Why: the integration tests should see the same ordering contract as the
        /// real store.
        /// </summary>
        public IReadOnlyList<SavedArticle> List(string userId)
        {
            // What: Lists saved articles in newest-first order.
            // How: Filters by user id, then sorts in-memory values by SavedAtUtc descending.
            // Why: The integration tests should see the same ordering contract as the real store.
            return _articles
                .Where(article => article.Key.StartsWith($"{userId}:", StringComparison.Ordinal))
                .Select(article => article.Value)
                .OrderByDescending(article => article.SavedAtUtc)
                .ToList();
        }

        /// <summary>
        /// What: Removes one fake saved article.
        /// How: deletes the composite user/article key from the dictionary.
        /// Why: the API DELETE test needs a real state change to verify.
        /// </summary>
        public bool Remove(string userId, string articleId)
        {
            // What: Removes one saved article by id.
            // How: Delegates to Dictionary.Remove.
            // Why: The API DELETE test needs a real state change to verify.
            return _articles.Remove($"{userId}:{articleId}");
        }
    }

    /// <summary>
    /// What: Fake news provider used by the integration-test API host.
    /// How: Returns deterministic article and source provider results from memory.
    /// Why: Integration tests need predictable data and must not spend NewsAPI quota or require an API key.
    /// </summary>
    private sealed class FakeNewsProvider : INewsProvider
    {
        public Task<ArticleProviderResult> GetArticlesAsync(
            ArticleQuery query,
            CancellationToken cancellationToken)
        {
            // What: Returns one article matching the requested page metadata.
            // How: Creates a domain Article and wraps it in ArticleProviderResult.
            // Why: The controller/use case should be tested with realistic domain data but without external HTTP calls.
            var article = CreateArticle();

            return Task.FromResult(new ArticleProviderResult(
                new[] { article },
                TotalResults: 1,
                FetchedAtUtc: DateTimeOffset.UtcNow,
                CachedUntilUtc: DateTimeOffset.UtcNow.AddMinutes(5),
                FromCache: false));
        }

        /// <summary>
        /// What: Returns deterministic fake source data.
        /// How: builds one Source from the incoming filter values and wraps it in a
        /// SourceProviderResult.
        /// Why: source endpoint tests need predictable data without external HTTP.
        /// </summary>
        public Task<SourceProviderResult> GetSourcesAsync(
            string? category,
            string? language,
            string? country,
            CancellationToken cancellationToken)
        {
            // What: Returns one source that echoes the normalized filters.
            // How: Creates a domain Source using the incoming filter values.
            // Why: Echoing filters proves the source request travelled through model binding and application normalization.
            var source = new Source(
                Id: "integration-source",
                Name: "Integration Source",
                Description: "Source returned by API integration tests.",
                Url: "https://example.com/source",
                Category: category,
                Language: language,
                Country: country);

            return Task.FromResult(new SourceProviderResult(
                new[] { source },
                FetchedAtUtc: DateTimeOffset.UtcNow,
                CachedUntilUtc: DateTimeOffset.UtcNow.AddMinutes(5),
                FromCache: false));
        }
    }

    /// <summary>
    /// What: Fake snapshot store used by the integration-test API host.
    /// How: Stores one known article id and supports the same Find/Remember methods as the real store.
    /// Why: Article detail route behavior can be tested without relying on SQLite state from previous article-list requests.
    /// </summary>
    private sealed class FakeArticleSnapshotStore : IArticleSnapshotStore
    {
        private readonly Dictionary<string, Article> _articles = new()
        {
            ["integration-article"] = CreateArticle()
        };

        public void Remember(IEnumerable<Article> articles, DateTimeOffset expiresAtUtc)
        {
            // What: Stores any articles remembered by the application during a test request.
            // How: Replaces dictionary values by article id.
            // Why: This keeps the fake close to the real overwrite behavior while remaining in memory.
            foreach (var article in articles)
            {
                _articles[article.Id] = article;
            }
        }

        /// <summary>
        /// What: Finds a fake article snapshot by id.
        /// How: returns the dictionary value or null for unknown ids.
        /// Why: null is the application contract that controllers map to HTTP 404.
        /// </summary>
        public Article? Find(string id)
        {
            // What: Finds a snapshot by id.
            // How: Uses Dictionary.GetValueOrDefault to return null for unknown ids.
            // Why: Null is the application contract that controllers map to HTTP 404.
            return _articles.GetValueOrDefault(id);
        }
    }

    /// <summary>
    /// What: Creates the shared article used by API integration fakes.
    /// How: Fills every required Article field with stable sample data.
    /// Why: Stable data keeps JSON assertions clear and avoids repeating article construction in multiple fakes.
    /// </summary>
    private static Article CreateArticle()
    {
        return new Article(
            Id: "integration-article",
            SourceName: "Integration Source",
            Author: "Integration Author",
            Title: "Integration test article",
            Description: "Article returned by API integration tests.",
            Url: "https://example.com/articles/integration-article",
            ImageUrl: "https://example.com/articles/integration-article.jpg",
            PublishedAt: new DateTimeOffset(2026, 7, 9, 12, 0, 0, TimeSpan.Zero),
            Content: "Integration test content.");
    }
}
