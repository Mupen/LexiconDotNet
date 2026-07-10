using ReactNews.Application.Contracts.EditorialArticles;
using ReactNews.Application.Interfaces;
using ReactNews.Application.Queries.EditorialArticles;
using ReactNews.Domain.Entities.EditorialArticles;
using ReactNews.Domain.Enums.EditorialArticles;

namespace ReactNews.UnitTests.Application;

/// <summary>
/// What: Tests editorial article use cases without using EF Core.
/// How: Uses an in-memory fake editorial store.
/// Why: Editorial validation and workflow behavior should be proven before persistence/API tests.
/// </summary>
public sealed class EditorialArticleUseCaseTests
{
    [Fact]
    public void CreateEditorialArticle_ReturnsSavedArticle_WhenRequestIsValid()
    {
        var store = new FakeEditorialArticleStore();
        var useCase = new CreateEditorialArticle(store);

        var result = useCase.Execute(CreateRequest());

        Assert.True(result.IsSuccess);
        Assert.Equal("Draft", result.Value?.Status);
        Assert.Single(store.List());
    }

    /// <summary>
    /// What: Verifies that invalid editorial titles fail validation.
    /// How: executes CreateEditorialArticle with a title shorter than the minimum
    /// allowed by the factory.
    /// Why: content validation should stop invalid articles before persistence.
    /// </summary>
    [Fact]
    public void CreateEditorialArticle_ReturnsValidationError_WhenTitleIsTooShort()
    {
        var useCase = new CreateEditorialArticle(new FakeEditorialArticleStore());

        var result = useCase.Execute(CreateRequest(title: "Bad"));

        Assert.True(result.IsFailure);
        Assert.Equal("validation_error", result.Error?.Code);
    }

    /// <summary>
    /// What: Verifies that an existing editorial article can be updated.
    /// How: creates an article, executes the update use case with a new title, and
    /// checks the returned DTO.
    /// Why: editors need to revise saved drafts without creating duplicate articles.
    /// </summary>
    [Fact]
    public void UpdateEditorialArticle_UpdatesExistingArticle()
    {
        var store = new FakeEditorialArticleStore();
        var create = new CreateEditorialArticle(store);
        var created = create.Execute(CreateRequest()).Value!;
        var update = new UpdateEditorialArticle(store);

        var result = update.Execute(created.Id, CreateRequest(title: "Updated editorial title"));

        Assert.True(result.IsSuccess);
        Assert.Equal("Updated editorial title", result.Value?.Title);
    }

    /// <summary>
    /// What: Verifies that the editorial status-change use case can publish content.
    /// How: creates a draft article and changes its status to Published.
    /// Why: publication is a workflow command and should set public-ready state and
    /// publication timestamp together.
    /// </summary>
    [Fact]
    public void ChangeEditorialArticleStatus_PublishesArticle()
    {
        var store = new FakeEditorialArticleStore();
        var created = new CreateEditorialArticle(store).Execute(CreateRequest()).Value!;
        var useCase = new ChangeEditorialArticleStatus(store);

        var result = useCase.Execute(created.Id, EditorialArticleStatus.Published);

        Assert.True(result.IsSuccess);
        Assert.Equal("Published", result.Value?.Status);
        Assert.NotNull(result.Value?.PublishedAtUtc);
    }

    /// <summary>
    /// What: Verifies that the public editorial list contains only published articles.
    /// How: creates one draft and one published article, then lists published content.
    /// Why: public reader views must not expose private draft content.
    /// </summary>
    [Fact]
    public void ListPublishedEditorialArticles_ReturnsOnlyPublishedArticles()
    {
        var store = new FakeEditorialArticleStore();
        var draft = new CreateEditorialArticle(store).Execute(CreateRequest(title: "Draft editorial article")).Value!;
        var published = new CreateEditorialArticle(store).Execute(CreateRequest(title: "Published editorial article")).Value!;
        new ChangeEditorialArticleStatus(store).Execute(published.Id, EditorialArticleStatus.Published);
        var useCase = new ListPublishedEditorialArticles(store);

        var response = useCase.Execute();

        Assert.Single(response.Items);
        Assert.Equal("Published editorial article", response.Items[0].Title);
        Assert.DoesNotContain(response.Items, article => article.Id == draft.Id);
    }

    /// <summary>
    /// What: Verifies that draft articles are hidden from public detail lookup.
    /// How: creates a draft and requests it through GetPublishedEditorialArticleById.
    /// Why: knowing an article id must not bypass the publishing workflow.
    /// </summary>
    [Fact]
    public void GetPublishedEditorialArticleById_ReturnsNull_WhenArticleIsDraft()
    {
        var store = new FakeEditorialArticleStore();
        var draft = new CreateEditorialArticle(store).Execute(CreateRequest()).Value!;
        var useCase = new GetPublishedEditorialArticleById(store);

        var response = useCase.Execute(draft.Id);

        Assert.Null(response);
    }

    /// <summary>
    /// What: Builds a valid editorial request for use-case tests.
    /// How: supplies all required fields and allows the title to be overridden.
    /// Why: tests can focus on one behavior while keeping the rest of the request
    /// valid under application validation rules.
    /// </summary>
    private static EditorialArticleRequest CreateRequest(string title = "Editorial article title")
    {
        return new EditorialArticleRequest(
            Title: title,
            Summary: "This is a valid editorial article summary.",
            Body: "This is a valid editorial article body with enough text to pass validation.",
            Author: "Admin Editor",
            Category: "technology",
            ImageUrl: null,
            Status: "Draft");
    }

    private sealed class FakeEditorialArticleStore : IEditorialArticleStore
    {
        private readonly Dictionary<string, EditorialArticle> _articles = new();

        /// <summary>
        /// What: Lists fake editorial articles.
        /// How: returns in-memory articles ordered by update timestamp descending.
        /// Why: use-case tests should see the same ordering contract as the real store.
        /// </summary>
        public IReadOnlyList<EditorialArticle> List()
        {
            return _articles.Values.OrderByDescending(article => article.UpdatedAtUtc).ToList();
        }

        /// <summary>
        /// What: Finds one fake editorial article by id.
        /// How: reads the in-memory dictionary and returns null when the id is absent.
        /// Why: use cases need realistic missing-article behavior without EF Core.
        /// </summary>
        public EditorialArticle? Find(string id)
        {
            return _articles.GetValueOrDefault(id);
        }

        /// <summary>
        /// What: Saves one fake editorial article.
        /// How: stores or replaces the article by id in an in-memory dictionary.
        /// Why: create/update/status tests need persistent state inside the test.
        /// </summary>
        public EditorialArticle Save(EditorialArticle article)
        {
            _articles[article.Id] = article;
            return article;
        }
    }
}
