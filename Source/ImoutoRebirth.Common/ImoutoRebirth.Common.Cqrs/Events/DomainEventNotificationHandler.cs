﻿using ImoutoRebirth.Common.Domain;
using MediatR;

namespace ImoutoRebirth.Common.Cqrs.Events;

public abstract class DomainEventNotificationHandler<TEvent> : INotificationHandler<EventWrapper<IDomainEvent>>
    where TEvent : IDomainEvent
{
    public Task Handle(EventWrapper<IDomainEvent> wrapper, CancellationToken cancellationToken)
    {
        if (!(wrapper.Wrapped is TEvent domainEvent))
            return Task.CompletedTask;

        return Handle(domainEvent, cancellationToken);
    }

    protected abstract Task Handle(TEvent domainEvent, CancellationToken cancellationToken);
}