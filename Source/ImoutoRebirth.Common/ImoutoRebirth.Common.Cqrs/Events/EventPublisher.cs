﻿using ImoutoRebirth.Common.Domain;
using MediatR;

namespace ImoutoRebirth.Common.Cqrs.Events;

public class EventPublisher : IEventPublisher
{
    private readonly IMediator _mediator;

    public EventPublisher(IMediator mediator)
    {
        _mediator = mediator;
    }

    public Task Publish<TEvent>(TEvent e, CancellationToken cancellationToken) 
        where TEvent : IDomainEvent
    {
        var wrapper = new EventWrapper<TEvent>(e);
        return _mediator.Publish(wrapper, cancellationToken);
    }
}