using ImoutoProject.Common.Cqrs.Behaviors;
using ImoutoProject.Common.Cqrs.Events;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoProject.Common.Cqrs
{
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
}