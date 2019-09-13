using System.Threading;
using System.Threading.Tasks;
using ImoutoRebirth.Common.Domain;
using MediatR;

namespace ImoutoProject.Common.Cqrs.Events
{
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
}