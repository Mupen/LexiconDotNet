using Microsoft.Extensions.DependencyInjection;
using WebCV.Application.Queries;
using WebCV.Application.UseCases;

namespace WebCV.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddWebCvApplication(this IServiceCollection services)
    {
        services.AddScoped<GetDefaultCvProfile>();
        services.AddScoped<ReplaceDefaultCvProfile>();

        return services;
    }
}
