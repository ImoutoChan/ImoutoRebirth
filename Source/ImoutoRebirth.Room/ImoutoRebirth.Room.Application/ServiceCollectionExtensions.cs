using System.Reflection;
using ImoutoRebirth.Common.Cqrs;
using ImoutoRebirth.Room.Application.Cqrs;
using ImoutoRebirth.Room.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Room.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRoomApplication(
        this IServiceCollection services,
        params Assembly[] registerAssemblies)
    {
        services.AddDefaultMediatR(x =>
        {
            x.RegisterServicesFromAssemblyContaining<OverseeCommandHandler>();
            x.RegisterServicesFromAssemblies(registerAssemblies);
        });
        services.AddLoggingBehavior();
        services.AddTransactionBehavior();

        return services;
    }
}
