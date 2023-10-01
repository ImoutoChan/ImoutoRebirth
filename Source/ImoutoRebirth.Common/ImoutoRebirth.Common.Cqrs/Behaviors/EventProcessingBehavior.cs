using ImoutoRebirth.Common.Cqrs.Events;
using ImoutoRebirth.Common.Domain;
using MediatR;

namespace ImoutoRebirth.Common.Cqrs.Behaviors;

internal class EventProcessingBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEventPublisher _eventPublisher;
    private readonly IEventStorage _eventStorage;

    public EventProcessingBehavior(IEventStorage eventStorage, IEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher;
        _eventStorage = eventStorage;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var mark = Guid.NewGuid();
        _eventStorage.Mark(mark);

        var response = await next();

        var events = _eventStorage.GetAll(mark);
        await InvokeEventsAsync(events, ct);

        return response;
    }

    private async Task InvokeEventsAsync(IReadOnlyCollection<IDomainEvent> events, CancellationToken ct)
    {
        foreach (var domainEvent in events)
        {
            await _eventPublisher.Publish(domainEvent, ct);
        }
    }
}
