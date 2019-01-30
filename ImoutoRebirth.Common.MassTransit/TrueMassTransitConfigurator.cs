using System;
using MassTransit.RabbitMqTransport;

namespace ImoutoRebirth.Common.MassTransit
{
    internal class TrueMassTransitConfigurator : ITrueMassTransitConfigurator
    {
        public string ApplicationName { get; }

        public IRabbitMqBusFactoryConfigurator RabbitMqBusFactoryConfigurator { get; }

        public IServiceProvider ServiceProvider { get; }

        public IRabbitMqHost RabbitMqHost { get; }

        public TrueMassTransitConfigurator(
            string applicationName, 
            IRabbitMqBusFactoryConfigurator rabbitMqBusFactoryConfigurator, 
            IServiceProvider serviceProvider, 
            IRabbitMqHost rabbitMqHost)
        {
            ApplicationName = applicationName;
            RabbitMqBusFactoryConfigurator = rabbitMqBusFactoryConfigurator;
            ServiceProvider = serviceProvider;
            RabbitMqHost = rabbitMqHost;
        }
    }
}