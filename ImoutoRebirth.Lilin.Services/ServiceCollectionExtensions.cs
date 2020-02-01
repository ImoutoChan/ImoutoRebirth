using ImoutoRebirth.Common.Cqrs;
using ImoutoRebirth.Common.MassTransit;
using ImoutoRebirth.Lilin.MessageContracts;
using ImoutoRebirth.Lilin.Services.CQRS.Commands;
using ImoutoRebirth.Lilin.Services.MessageCommandHandlers;
using ImoutoRebirth.Meido.MessageContracts;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ReceiverApp = ImoutoRebirth.Meido.MessageContracts.ReceiverApp;

namespace ImoutoRebirth.Lilin.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLilinServices(this IServiceCollection services)
        {
            services.AddMediatR(typeof(SaveMetadataCommand));
            services.AddLoggingBehavior();
            services.AddTransactionBehavior();

            services.AddTransient<UpdateMetadataCommandConsumer>();

            return services;
        }
        
        public static ITrueMassTransitConfigurator AddLilinServicesForRabbit(
            this ITrueMassTransitConfigurator builder)
        {
            builder.AddConsumer<UpdateMetadataCommandConsumer, IUpdateMetadataCommand>()
                   .AddFireAndForget<ISavedCommand>(ReceiverApp.Name);

            return builder;
        }
    }
}