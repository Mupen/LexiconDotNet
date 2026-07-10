using ReactNews.Application.Interfaces;

namespace ReactNews.Infrastructure.Providers;

/// <summary>
/// Infrastructure-only marker for the uncached news provider.
/// </summary>
/// <remarks>
/// What: identifies the raw provider that performs external I/O.
/// Why: the application should receive the cached INewsProvider, but the cache
/// decorator needs a separate dependency so it can wrap the raw provider without
/// causing a circular dependency in dependency injection.
/// How: NewsApiClient implements this interface. DI registers it as the raw
/// source and registers CachedNewsFeedClient as the application's
/// INewsProvider implementation.
/// </remarks>
public interface INewsProviderSource : INewsProvider
{
}
