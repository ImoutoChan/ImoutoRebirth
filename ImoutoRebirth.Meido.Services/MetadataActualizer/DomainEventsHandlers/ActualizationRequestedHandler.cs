using System.Threading;
using System.Threading.Tasks;
using ImoutoRebirth.Common.Cqrs.Events;
using ImoutoRebirth.Arachne.MessageContracts;
using ImoutoRebirth.Arachne.MessageContracts.Commands;
using ImoutoRebirth.Meido.Core.SourceActualizingState;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Meido.Services.MetadataActualizer.DomainEventsHandlers
{
    public class ActualizationRequestedHandler : DomainEventNotificationHandler<ActualizationRequested>
    {
        private readonly ILogger<ActualizationRequestedHandler> _logger;
        private readonly IBus _bus;

        public ActualizationRequestedHandler(ILogger<ActualizationRequestedHandler> logger, IBus bus)
        {
            _logger = logger;
            _bus = bus;
        }

        protected override async Task Handle(ActualizationRequested domainEvent, CancellationToken cancellationToken)
        {
            var searchEngineType = (SearchEngineType) domainEvent.State.Source;

            _logger.LogInformation(
                "Sending request to update tags and notes history from {MetadataSource}",
                searchEngineType);

            var tagCommand
                = new
                  {
                      SearchEngineType = searchEngineType,
                      LastProcessedTagHistoryId = domainEvent.State.LastProcessedTagHistoryId
                  };

            var noteCommand
                = new
                  {
                      SearchEngineType = searchEngineType,
                      LastProcessedNoteUpdateAt = domainEvent.State.LastProcessedNoteUpdateAt
                  };

            await _bus.Send<ILoadTagHistoryCommand>(tagCommand, cancellationToken);
            await _bus.Send<ILoadNoteHistoryCommand>(noteCommand, cancellationToken);
        }
    }
}
