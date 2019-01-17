using ImoutoRebirth.Arachne.MessageContracts.Commands;
using ImoutoRebirth.Arachne.Service.Consumers;
using ImoutoRebirth.Common.MassTransit;
using ImoutoRebirth.Lilin.MessageContracts;
using MassTransit.RabbitMq.Extensions.Hosting.Contracts;
using MassTransit.RabbitMq.Extensions.Hosting.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Arachne.Service.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddArachneServices(this IServiceCollection services)
        {
            services.AddTransient<ISearchMetadataCommandHandler, SearchMetadataCommandHandler>();
            services.AddTransient<IRemoteCommandService, RemoteCommandService>();

            return services;
        }

        public static IMassTransitRabbitMqHostingBuilder AddArachneServicesForRabbit(
            this IMassTransitRabbitMqHostingBuilder builder)
            => builder.AddDefaultConsumer<EverywhereSearchMetadataCommandConsumer, IEverywhereSearchMetadataCommand>()
                      .AddDefaultConsumer<YandereSearchMetadataCommandConsumer, IYandereSearchMetadataCommand>()
                      .AddDefaultConsumer<DanbooruSearchMetadataCommandConsumer, IDanbooruSearchMetadataCommand>()
                      .AddDefaultConsumer<SankakuSearchMetadataCommandConsumer, ISankakuSearchMetadataCommand>()
                      .WithFireAndForgetSendEndpointByConvention<IUpdateMetadataCommand>(ReceiverApp.Name);
    }
}