using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Common.MassTransit;

public static class BusControlExtensions
{
    public static HostReceiveEndpointHandle ConnectNewConsumer<TConsumer>(
        this IBusControl busControl, 
        IServiceProvider serviceProvider)
        where TConsumer : class, IConsumer
    {
        var formatter = new ImoutoRebirthEndpointNameFormatter(typeof(TConsumer));
        var queueName = formatter.Consumer<TConsumer>();

        return busControl.ConnectReceiveEndpoint(queueName, e =>
        {
            e.PrefetchCount = 1;
            e.UseMessageRetry(x => x.Intervals(30, 60, 120));
            e.Consumer(() => serviceProvider.GetRequiredService<TConsumer>());
        });
    }
}
