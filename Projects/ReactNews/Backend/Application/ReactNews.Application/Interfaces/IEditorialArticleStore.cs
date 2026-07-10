using ReactNews.Domain.Entities.EditorialArticles;

namespace ReactNews.Application.Interfaces;

/// <summary>
/// What: Stores and retrieves admin-created editorial articles.
/// How: Application use cases depend on this interface while Infrastructure provides the EF implementation.
/// Why: Editorial business flow should not depend directly on SQLite or EF Core.
/// </summary>
public interface IEditorialArticleStore
{
    IReadOnlyList<EditorialArticle> List();

    EditorialArticle? Find(string id);

    EditorialArticle Save(EditorialArticle article);
}
