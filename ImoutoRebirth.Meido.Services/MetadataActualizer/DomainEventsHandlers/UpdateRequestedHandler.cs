using System.Threading;
using System.Threading.Tasks;
using ImoutoRebirth.Common.Cqrs.Events;
using ImoutoRebirth.Meido.Core.ParsingStatus.Events;
using ImoutoRebirth.Meido.Services.MetadataRequest;

namespace ImoutoRebirth.Meido.Services.MetadataActualizer.DomainEventsHandlers
{
    public class UpdateRequestedHandler : DomainEventNotificationHandler<UpdateRequested>
    {
        private readonly IMetadataRequesterProvider _metadataRequesterProvider;

        public UpdateRequestedHandler(IMetadataRequesterProvider metadataRequesterProvider)
        {
            _metadataRequesterProvider = metadataRequesterProvider;
        }

        protected override async Task Handle(UpdateRequested domainEvent, CancellationToken cancellationToken) 
            => await _metadataRequesterProvider.Get(domainEvent.Entity.Source)
                                               .SendRequestCommand(
                                                    domainEvent.Entity.FileId,
                                                    domainEvent.Entity.Md5);
    }
}