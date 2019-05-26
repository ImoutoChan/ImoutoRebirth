using System.Threading;
using System.Threading.Tasks;
using ImoutoProject.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Core.Services;
using MediatR;

namespace ImoutoRebirth.Lilin.Services.CQRS.Commands
{
    public class SaveMetadataCommandHandler : ICommandHandler<SaveMetadataCommand>
    {
        private readonly IMetadataUpdateService _metadataUpdateService;

        public SaveMetadataCommandHandler(IMetadataUpdateService metadataUpdateService)
        {
            _metadataUpdateService = metadataUpdateService;
        }

        public async Task<Unit> Handle(SaveMetadataCommand request, CancellationToken cancellationToken)
        {
            var metadata = request.Update;

            await _metadataUpdateService.ApplyMetadata(metadata);

            return Unit.Value;
        }
    }
}