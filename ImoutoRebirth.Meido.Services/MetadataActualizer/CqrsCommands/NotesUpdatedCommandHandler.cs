using System.Threading;
using System.Threading.Tasks;
using ImoutoProject.Common.Cqrs.Abstract;
using ImoutoRebirth.Meido.Core.SourceActualizingState;
using MediatR;

namespace ImoutoRebirth.Meido.Services.MetadataActualizer.CqrsCommands
{
    internal class NotesUpdatedCommandHandler : ICommandHandler<NotesUpdatedCommand>
    {
        private readonly ISourceActualizerService _sourceActualizerService;

        public NotesUpdatedCommandHandler(ISourceActualizerService sourceActualizerService)
        {
            _sourceActualizerService = sourceActualizerService;
        }

        public async Task<Unit> Handle(NotesUpdatedCommand request, CancellationToken cancellationToken)
        {
            await _sourceActualizerService.MarkNotesUpdated(
                request.SourceId,
                request.PostIds,
                request.LastNoteUpdateDate);

            return Unit.Value;
        }
    }
}