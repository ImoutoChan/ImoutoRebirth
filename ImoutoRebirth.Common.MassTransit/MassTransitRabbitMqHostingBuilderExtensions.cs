using System;
using GreenPipes;
using GreenPipes.Configurators;
using MassTransit;
using MassTransit.RabbitMq.Extensions.Hosting.Contracts;
using MassTransit.RabbitMq.Extensions.Hosting.Extensions;
using MassTransit.RabbitMqTransport;

namespace ImoutoRebirth.Common.MassTransit
{
    public static class MassTransitRabbitMqHostingBuilderExtensions
    {
        public static IMassTransitRabbitMqHostingBuilder AddDefaultConsumer<TConsumer, TCommand>(
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
