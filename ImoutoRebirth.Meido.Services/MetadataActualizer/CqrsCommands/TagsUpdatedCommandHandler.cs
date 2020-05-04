using System.Threading;
using System.Threading.Tasks;
using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Meido.Core.SourceActualizingState;
using MediatR;

namespace ImoutoRebirth.Meido.Services.MetadataActualizer.CqrsCommands
{
    internal class TagsUpdatedCommandHandler : ICommandHandler<TagsUpdatedCommand>
    {
        private readonly ISourceActualizerService _sourceActualizerService;

        public TagsUpdatedCommandHandler(ISourceActualizerService sourceActualizerService)
        {
            _sourceActualizerService = sourceActualizerService;
        }

        public async Task<Unit> Handle(TagsUpdatedCommand request, CancellationToken cancellationToken)
        {
            await _sourceActualizerService.MarkTagsUpdated(request.SourceId, request.PostIds, request.LastHistoryId);

            return Unit.Value;
        }
    }
}