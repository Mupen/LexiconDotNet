namespace ReactNews.Application.Contracts.EditorialArticles;

/// <summary>
/// What: Request contract for creating or updating editorial articles.
/// How: Carries text fields, metadata, and optional requested status from the admin UI.
/// Why: Controllers should bind HTTP JSON and let Application validate the editorial rules.
/// </summary>
public sealed record EditorialArticleRequest(
    string? Title,
    string? Summary,
    string? Body,
    string? Author,
    string? Category,
    string? ImageUrl,
    string? Status);
