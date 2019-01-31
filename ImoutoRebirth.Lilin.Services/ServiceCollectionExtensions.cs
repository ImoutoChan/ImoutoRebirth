using ImoutoRebirth.Common.MassTransit;
using ImoutoRebirth.Lilin.MessageContracts;
using ImoutoRebirth.Lilin.Services.Behaviors;
using ImoutoRebirth.Lilin.Services.MessageCommandHandlers;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Lilin.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLilinServices(this IServiceCollection services)
        {
            services.AddMediatR();
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
            services.AddTransient<UpdateMetadataCommandConsumer>();

            return services;
        }
        
        public static ITrueMassTransitConfigurator AddLilinServicesForRabbit(
            this ITrueMassTransitConfigurator builder)
        {
            builder.AddConsumer<UpdateMetadataCommandConsumer, IUpdateMetadataCommand>();

            return builder;
        }
    }
}