using Microsoft.Extensions.DependencyInjection;
using ReactNews.Application.Queries.Auth;
using ReactNews.Application.Queries.Articles;
using ReactNews.Application.Queries.EditorialArticles;
using ReactNews.Application.Queries.ReaderPreferences;
using ReactNews.Application.Queries.SavedArticles;
using ReactNews.Application.Queries.Sources;

namespace ReactNews.Application;

/// <summary>
/// Registers application-layer use cases.
/// </summary>
/// <remarks>
/// What: this method tells dependency injection how to create application
/// services such as GetArticles and GetSources.
/// Why: Program.cs should not manually know every use case class. Keeping this
/// list in the Application project makes the dependency direction obvious:
/// API depends on Application, not the other way around.
/// How: scoped lifetime gives each HTTP request its own use case instances while
/// still allowing those use cases to depend on scoped infrastructure services.
/// </remarks>
public static class ApplicationDependencyInjection
{
    /// <summary>
    /// Registers application use cases.
    /// </summary>
    /// <remarks>
    /// What: adds each query/use case class to the service collection.
    /// How: each class is registered as scoped so one HTTP request gets one use
    /// case instance.
    /// Why: scoped lifetime matches ASP.NET request handling and allows use cases
    /// to depend on scoped infrastructure such as EF Core-backed snapshot storage.
    /// </remarks>
    public static IServiceCollection AddReactNewsApplication(this IServiceCollection services)
    {
        services.AddScoped<GetArticles>();
        services.AddScoped<GetArticleById>();
        services.AddScoped<GetSources>();
        services.AddScoped<ListSavedArticles>();
        services.AddScoped<SaveArticleForLater>();
        services.AddScoped<RemoveSavedArticle>();
        services.AddScoped<GetReaderPreferences>();
        services.AddScoped<UpdateReaderPreferences>();
        services.AddScoped<ListEditorialArticles>();
        services.AddScoped<GetEditorialArticleById>();
        services.AddScoped<ListPublishedEditorialArticles>();
        services.AddScoped<GetPublishedEditorialArticleById>();
        services.AddScoped<CreateEditorialArticle>();
        services.AddScoped<UpdateEditorialArticle>();
        services.AddScoped<ChangeEditorialArticleStatus>();
        services.AddScoped<RegisterUser>();
        services.AddScoped<LoginUser>();
        services.AddScoped<GetCurrentUser>();
        services.AddScoped<UpdateProfile>();
        services.AddScoped<ChangePassword>();
        services.AddScoped<DeleteAccount>();

        return services;
    }
}
