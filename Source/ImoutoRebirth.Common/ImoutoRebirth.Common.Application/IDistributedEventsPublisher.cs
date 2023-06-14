namespace ImoutoRebirth.Common.Application;

public interface IDistributedEventsPublisher
{
    Task PublishAsync<TEvent>(object @event, CancellationToken token = default)
        where TEvent : class;
}
