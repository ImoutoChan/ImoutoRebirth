using System;
using GreenPipes;
using GreenPipes.Configurators;
using ImoutoRebirth.Lilin.MessageContracts;
using ImoutoRebirth.Lilin.Services.MessageCommandHandlers;
using MassTransit;
using MassTransit.RabbitMq.Extensions.Hosting.Contracts;
using MassTransit.RabbitMq.Extensions.Hosting.Extensions;
using MassTransit.RabbitMqTransport;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Lilin.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLilinServices(this IServiceCollection services)
        {
            services.AddMediatR();

            return services;
        }

        public static IMassTransitRabbitMqHostingBuilder AddLilinServicesForRabbit(
            this IMassTransitRabbitMqHostingBuilder builder)
            => builder.AddDefaultConsumer<UpdateMetadataCommandConsumer, IUpdateMetadataCommand>();


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