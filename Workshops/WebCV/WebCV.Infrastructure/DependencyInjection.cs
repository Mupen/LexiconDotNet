using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebCV.Application.Interfaces;
using WebCV.Infrastructure.Persistence;
using WebCV.Infrastructure.Repositories;

namespace WebCV.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddWebCvInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<WebCvDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<ICvProfileRepository, EfCvProfileRepository>();

        return services;
    }
}
