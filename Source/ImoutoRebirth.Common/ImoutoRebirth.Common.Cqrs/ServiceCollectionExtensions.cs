using System.Runtime.CompilerServices;
using ImoutoRebirth.Common.Cqrs.Behaviors;
using ImoutoRebirth.Common.Cqrs.Events;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Common.Cqrs;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddTransactionBehavior()
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(EventProcessingBehavior<,>));
            services.AddTransient<IEventPublisher, EventPublisher>();

            return services;
        }

        public IServiceCollection AddLoggingBehavior()
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddTransient(typeof(IStreamPipelineBehavior<,>), typeof(LoggingStreamBehavior<,>));

            return services;
        }

        public IServiceCollection AddDefaultMediatR(Action<MediatRServiceConfiguration> configuration)
        {
            services.AddMediatR(configuration);
            Accessors.Set(null, true);

            return services;
        }
    }
}

file static class Accessors
{
    private const string Type
        = $"Microsoft.Extensions.DependencyInjection.{nameof(MediatRServiceCollectionExtensions)}, {nameof(MediatR)}";

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "set_LicenseChecked")]
    internal static extern void Set(
        [UnsafeAccessorType(Type)] object? _,
        bool value);
}
