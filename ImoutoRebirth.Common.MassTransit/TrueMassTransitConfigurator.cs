using MassTransit.RabbitMqTransport;

namespace ImoutoRebirth.Common.MassTransit;

internal class TrueMassTransitConfigurator : ITrueMassTransitConfigurator
{
    public IRabbitMqBusFactoryConfigurator RabbitMqBusFactoryConfigurator { get; }

    public IServiceProvider ServiceProvider { get; }

    public TrueMassTransitConfigurator(
        IRabbitMqBusFactoryConfigurator rabbitMqBusFactoryConfigurator,
        IServiceProvider serviceProvider)
    {
        RabbitMqBusFactoryConfigurator = rabbitMqBusFactoryConfigurator;
        ServiceProvider = serviceProvider;
    }
}