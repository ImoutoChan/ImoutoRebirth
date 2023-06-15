using System.Reflection;
using ImoutoRebirth.Common.Cqrs;
using ImoutoRebirth.Lilin.Application.TagSlice;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Lilin.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLilinApplication(
        this IServiceCollection services,
        params Assembly[] registerAssemblies)
    {
        services.AddMediatR(x =>
        {
            x.RegisterServicesFromAssemblyContaining<CreateTagCommandHandler>();
            x.RegisterServicesFromAssemblies(registerAssemblies);
        });
        services.AddLoggingBehavior();
        services.AddTransactionBehavior();

        services.AddMemoryCache();

        return services;
    }
}
