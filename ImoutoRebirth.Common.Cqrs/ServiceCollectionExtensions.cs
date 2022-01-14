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
        services.AddTransient<IEventPublisher, EventPublisher>();

        return services;
    }

    public static IServiceCollection AddLoggingBehavior(this IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        return services;
    }
}