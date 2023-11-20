using System.Reflection;
using ImoutoRebirth.Common.Cqrs;
using ImoutoRebirth.Room.Application.Cqrs;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Room.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRoomApplication(
        this IServiceCollection services,
        params Assembly[] registerAssemblies)
    {
        services.AddMediatR(x =>
        {
            x.RegisterServicesFromAssemblyContaining<OverseeCommandHandler>();
            x.RegisterServicesFromAssemblies(registerAssemblies);
        });
        services.AddLoggingBehavior();
        services.AddTransactionBehavior();

        return services;
    }
}
