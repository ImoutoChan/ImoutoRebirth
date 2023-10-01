using MassTransit;

namespace ImoutoRebirth.Common.MassTransit;

internal class TrueMassTransitConfigurator : ITrueMassTransitConfigurator
{
    public IRabbitMqBusFactoryConfigurator RabbitMqBusFactoryConfigurator { get; }

    public IBusRegistrationContext BusRegistrationContext { get; }

    public TrueMassTransitConfigurator(
        IRabbitMqBusFactoryConfigurator rabbitMqBusFactoryConfigurator,
        IBusRegistrationContext serviceProvider)
    {
        RabbitMqBusFactoryConfigurator = rabbitMqBusFactoryConfigurator;
        BusRegistrationContext = serviceProvider;
    }
}
