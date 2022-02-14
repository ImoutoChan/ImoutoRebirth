using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Arachne.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddArachneCore(this IServiceCollection services)
    {
        services.AddTransient<IArachneSearchService, ArachneSearchService>();

        return services;
    }
}