using System.Threading;
using System.Threading.Tasks;
using ImoutoProject.Common.Cqrs.Abstract;
using ImoutoRebirth.Meido.Core.SourceActualizingState;
using MediatR;

namespace ImoutoRebirth.Meido.Services.MetadataActualizer.CqrsCommands
{
    internal class ActualizeSourcesCommandHandler : ICommandHandler<ActualizeSourcesCommand>
    {
        private readonly ISourceActualizerService _sourceActualizerService;

        public ActualizeSourcesCommandHandler(ISourceActualizerService sourceActualizerService)
        {
            _sourceActualizerService = sourceActualizerService;
        }

        public async Task<Unit> Handle(ActualizeSourcesCommand request, CancellationToken cancellationToken)
        {
            await _sourceActualizerService.RequestActualization();

            return Unit.Value;
        }
    }
}