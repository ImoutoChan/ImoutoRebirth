using System.Reflection;
using ImoutoRebirth.Common.Cqrs.Behaviors;
using ImoutoRebirth.Common.Cqrs.Events;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Common.Cqrs;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTransactionBehavior(this IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(EventProcessingBehavior<,>));
        services.AddTransient<IEventPublisher, EventPublisher>();

        return services;
    }

    public static IServiceCollection AddLoggingBehavior(this IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IStreamPipelineBehavior<,>), typeof(LoggingStreamBehavior<,>));

        return services;
    }

    public static IServiceCollection AddDefaultMediatR(
        this IServiceCollection services,
        Action<MediatRServiceConfiguration> configuration)
    {
        var licenseFixProperties = typeof(Mediator).Assembly.GetTypes()
            .Where(x => x.Name == "ServiceCollectionExtensions")
            .SelectMany(x => x
                .GetProperties(BindingFlags.Static | BindingFlags.NonPublic)
                .Where(y => y.Name == "LicenseChecked"));

        foreach (var licenseFixProperty in licenseFixProperties)
            licenseFixProperty.SetValue(null, true);

        services.AddMediatR(configuration);

        return services;
    }
}
