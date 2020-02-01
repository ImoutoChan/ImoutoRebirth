using System.Threading;
using System.Threading.Tasks;
using ImoutoRebirth.Common.Cqrs.Events;
using ImoutoRebirth.Lilin.Core.Events;
using ImoutoRebirth.Lilin.Core.Models;
using ImoutoRebirth.Meido.MessageContracts;
using MassTransit;

namespace ImoutoRebirth.Lilin.Services.DomainEventHandlers
{
    public class MetadataUpdatedEventHandler : DomainEventNotificationHandler<MetadataUpdated>
    {
        private readonly IBus _bus;

        public MetadataUpdatedEventHandler(IBus bus)
        {
            _bus = bus;
        }

        protected override async Task Handle(MetadataUpdated domainEvent, CancellationToken cancellationToken)
        {
            if (domainEvent.MetadataSource == MetadataSource.Manual)
                return;

            var command = new
            {
                FileId = domainEvent.FileId,
                SourceId = (int) domainEvent.MetadataSource
            };

            await _bus.Send<ISavedCommand>(command, cancellationToken: cancellationToken);
        }
    }
}