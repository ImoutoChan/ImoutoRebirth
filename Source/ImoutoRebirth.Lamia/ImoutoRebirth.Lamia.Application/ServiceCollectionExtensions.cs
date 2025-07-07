using System.Reflection;
using ImoutoRebirth.Common.Cqrs;
using ImoutoRebirth.Lamia.Application.Cqrs;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Lamia.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLamiaApplication(
        this IServiceCollection services,
        params Assembly[] registerAssemblies)
    {
        services.AddDefaultMediatR(x =>
        {
            x.RegisterServicesFromAssemblies(registerAssemblies);
            x.RegisterServicesFromAssemblyContaining<ExtractMetadataCommand>();
        });
        services.AddLoggingBehavior();

        return services;
    }
}
