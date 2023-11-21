using MassTransit;

namespace ImoutoRebirth.Common.MassTransit;

internal class TrueMassTransitConfigurator : ITrueMassTransitConfigurator
{
    public IBusFactoryConfigurator BusFactoryConfigurator { get; }

    public IRabbitMqBusFactoryConfigurator? RabbitMqBusFactoryConfigurator =>
        BusFactoryConfigurator as IRabbitMqBusFactoryConfigurator;

    public IBusRegistrationContext BusRegistrationContext { get; }

    public TrueMassTransitConfigurator(
        IBusFactoryConfigurator busFactoryConfigurator,
        IBusRegistrationContext serviceProvider)
    {
        BusFactoryConfigurator = busFactoryConfigurator;
        BusRegistrationContext = serviceProvider;
    }
}
