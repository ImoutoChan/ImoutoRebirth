using MassTransit;

namespace ImoutoRebirth.Common.MassTransit;

public interface ITrueMassTransitConfigurator
{
    IRabbitMqBusFactoryConfigurator RabbitMqBusFactoryConfigurator { get; }

    IBusRegistrationContext BusRegistrationContext { get; }
}
