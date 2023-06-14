using ImoutoRebirth.Common.Application;
using MassTransit;

namespace ImoutoRebirth.Common.Infrastructure;

internal class DistributedEventsPublisher : IDistributedEventsPublisher
{
    private readonly IBus _bus;

    public DistributedEventsPublisher(IBus bus) => _bus = bus;

    public async Task PublishAsync<TEvent>(object @event, CancellationToken token = default)
        where TEvent : class 
        => await _bus.Publish<TEvent>(@event, token);
}
