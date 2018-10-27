using System;
using GreenPipes;
using GreenPipes.Configurators;
using ImoutoRebirth.Arachne.MessageContracts.Commands;
using ImoutoRebirth.Arachne.Service.Consumers;
using MassTransit;
using MassTransit.RabbitMq.Extensions.Hosting.Contracts;
using MassTransit.RabbitMq.Extensions.Hosting.Extensions;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Arachne.Service
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddArachneServices(this IServiceCollection services)
        {
            services.AddTransient<ISearchMetadataCommandHandler, SearchMetadataCommandHandler>();

            return services;
        }

        public static IMassTransitRabbitMqHostingBuilder AddArachneServicesForRabbit(
            this IMassTransitRabbitMqHostingBuilder builder)
            => builder.AddDefaultConsumer<EverywhereSearchMetadataCommandConsumer, IEverywhereSearchMetadataCommand>()
                      .AddDefaultConsumer<YandereSearchMetadataCommandConsumer, IYandereSearchMetadataCommand>()
                      .AddDefaultConsumer<DanbooruSearchMetadataCommandConsumer, IDanbooruSearchMetadataCommand>()
                      .AddDefaultConsumer<SankakuSearchMetadataCommandConsumer, ISankakuSearchMetadataCommand>();

        private static IMassTransitRabbitMqHostingBuilder AddDefaultConsumer<TConsumer, TCommand>(
            this IMassTransitRabbitMqHostingBuilder builder)
            where TCommand : class where TConsumer : class, IConsumer<TCommand>
            => builder.ConsumeByConvention<TConsumer, TCommand>(GetRetryPolicy, GetReceiveEndpointConfigurator);

        private static void GetReceiveEndpointConfigurator(IRabbitMqReceiveEndpointConfigurator configurator)
        {
            configurator.PrefetchCount = 1;
        }

        private static void GetRetryPolicy(IRetryConfigurator retryConfigurator)
            => retryConfigurator.Intervals(TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(60));
    }
}