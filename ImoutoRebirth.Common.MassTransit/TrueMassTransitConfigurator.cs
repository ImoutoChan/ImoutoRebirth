using System;
using MassTransit.RabbitMqTransport;

namespace ImoutoRebirth.Common.MassTransit
{
    internal class TrueMassTransitConfigurator : ITrueMassTransitConfigurator
    {
        public IRabbitMqBusFactoryConfigurator RabbitMqBusFactoryConfigurator { get; }

        public IServiceProvider ServiceProvider { get; }

        public IRabbitMqHost RabbitMqHost { get; }

        public TrueMassTransitConfigurator(
            IRabbitMqBusFactoryConfigurator rabbitMqBusFactoryConfigurator, 
            IServiceProvider serviceProvider, 
            IRabbitMqHost rabbitMqHost)
        {
            RabbitMqBusFactoryConfigurator = rabbitMqBusFactoryConfigurator;
            ServiceProvider = serviceProvider;
            RabbitMqHost = rabbitMqHost;
        }
    }
}