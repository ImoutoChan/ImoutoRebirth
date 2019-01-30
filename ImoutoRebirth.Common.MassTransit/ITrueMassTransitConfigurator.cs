using System;
using MassTransit.RabbitMqTransport;

namespace ImoutoRebirth.Common.MassTransit
{
    public interface ITrueMassTransitConfigurator
    {
        string ApplicationName { get; }

        IRabbitMqBusFactoryConfigurator RabbitMqBusFactoryConfigurator { get; }

        IServiceProvider ServiceProvider { get; }

        IRabbitMqHost RabbitMqHost { get; }
    }
}