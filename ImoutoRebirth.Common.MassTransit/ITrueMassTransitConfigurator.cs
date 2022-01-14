using System;
using MassTransit.RabbitMqTransport;

namespace ImoutoRebirth.Common.MassTransit;

public interface ITrueMassTransitConfigurator
{
    IRabbitMqBusFactoryConfigurator RabbitMqBusFactoryConfigurator { get; }

    IServiceProvider ServiceProvider { get; }
}