using System.Threading;
using System.Threading.Tasks;
using ImoutoProject.Common.Cqrs.Events;
using ImoutoRebirth.Meido.Core.ParsingStatus;
using ImoutoRebirth.Meido.Services.MetadataRequest;

namespace ImoutoRebirth.Meido.Services
{
    public class ParsingStatusCreatedHandler : DomainEventNotificationHandler<ParsingStatusCreated>
    {
        private readonly IMetadataRequesterProvider _metadataRequesterProvider;

        public ParsingStatusCreatedHandler(IMetadataRequesterProvider metadataRequesterProvider)
        {
            _metadataRequesterProvider = metadataRequesterProvider;
        }

        protected override async Task Handle(ParsingStatusCreated domainEvent, CancellationToken cancellationToken) 
            => await _metadataRequesterProvider.Get(domainEvent.Created.Source)
                                               .SendRequestCommand(
                                                    domainEvent.Created.FileId, 
                                                    domainEvent.Created.Md5);
    }
}