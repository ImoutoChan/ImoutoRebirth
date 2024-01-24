using MassTransit;

namespace ImoutoRebirth.Common.MassTransit;

public class MassTransitConfigurator
{
    public Action<IBusRegistrationContext, IBusFactoryConfigurator>? ConfigureCustomEndpoint { get; private set; }
    
    /// <summary>
    /// Register command to have an ability to send it without specifying endpoint.
    /// </summary>
    public MassTransitConfigurator AddCommand<T>() where T : class
    {
        var formatter = new ImoutoRebirthEndpointNameFormatter(typeof(IConsumer<T>));
        var queueName = formatter.Consumer<IConsumer<T>>();
        
        // we can't use exchange with db transport
        EndpointConvention.Map<T>(new Uri($"queue:{queueName}"));

        return this;
    }

    /// <summary>
    /// Should only be used for event consumers.
    /// </summary>
    public MassTransitConfigurator AddTemporaryConsumer<T>() where T : class, IConsumer
    {
        var current = ConfigureCustomEndpoint;
        ConfigureCustomEndpoint = (context, configurator) =>
        {
            current?.Invoke(context, configurator);
            configurator.ReceiveEndpoint(new TemporaryEndpointDefinition(), e =>
            {
                e.ConfigureConsumer<T>(context);
            });
        };

        return this;
    }
}
