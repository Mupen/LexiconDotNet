using Microsoft.Extensions.DependencyInjection;

namespace DataDrivenCaching.Application;

// WHAT:
// This extension method is the Application layer's composition hook.
//
// WHY:
// Keeping registration behind a named method lets Program.cs explain the app
// structure without listing every service in one file. The Application layer is
// currently small, but this gives us a clean place for use cases later.
public static class DependencyInjection
{
    public static IServiceCollection AddDataDrivenCachingApplication(this IServiceCollection services)
    {
        return services;
    }
}
