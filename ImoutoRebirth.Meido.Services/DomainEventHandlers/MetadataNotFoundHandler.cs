using System.Threading;
using System.Threading.Tasks;
using ImoutoRebirth.Common.Cqrs.Events;
using ImoutoRebirth.Meido.Core.ParsingStatus.Events;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Meido.Services.DomainEventHandlers
{
    public class MetadataNotFoundHandler : DomainEventNotificationHandler<MetadataNotFound>
    {
        private readonly ILogger<MetadataNotFoundHandler> _logger;

        public MetadataNotFoundHandler(ILogger<MetadataNotFoundHandler> logger)
        {
            _logger = logger;
        }

        protected override Task Handle(MetadataNotFound domainEvent, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Sending request to search original");
            
            // todo: send search original request
            return Task.CompletedTask;
        }
    }
}