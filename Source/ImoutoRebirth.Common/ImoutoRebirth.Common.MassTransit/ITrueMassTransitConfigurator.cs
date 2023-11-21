using MassTransit;

namespace ImoutoRebirth.Common.MassTransit;

public interface ITrueMassTransitConfigurator
{
    IBusFactoryConfigurator BusFactoryConfigurator { get; }

    IBusRegistrationContext BusRegistrationContext { get; }

    IRabbitMqBusFactoryConfigurator? RabbitMqBusFactoryConfigurator { get; }
}
