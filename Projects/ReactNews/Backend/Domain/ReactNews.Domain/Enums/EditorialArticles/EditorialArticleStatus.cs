namespace ReactNews.Domain.Enums.EditorialArticles;

/// <summary>
/// What: Represents the lifecycle state of an admin-created editorial article.
/// How: Draft is work-in-progress, Review is ready for checking, Published is public, and Archived is removed from active publication.
/// Why: Editorial content needs a workflow that is separate from external NewsAPI article snapshots.
/// </summary>
public enum EditorialArticleStatus
{
    Draft = 0,
    Review = 1,
    Published = 2,
    Archived = 3
}
