using ImoutoRebirth.Common.Domain;
using MediatR;

namespace ImoutoRebirth.Common.Cqrs.Events;

public class EventWrapper<TWrapped> : INotification where TWrapped : IDomainEvent
{
    public TWrapped Wrapped { get; }

    public EventWrapper(TWrapped wrapped)
    {
        Wrapped = wrapped;
    }
}