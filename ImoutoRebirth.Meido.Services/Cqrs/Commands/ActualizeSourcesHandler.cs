using System.Threading;
using System.Threading.Tasks;
using ImoutoProject.Common.Cqrs.Abstract;
using ImoutoRebirth.Meido.Core.SourceActualizingState;
using MediatR;

namespace ImoutoRebirth.Meido.Services.Cqrs.Commands
{
    internal class ActualizeSourcesHandler : ICommandHandler<ActualizeSources>
    {
        private readonly ISourceActualizer _sourceActualizer;

        public ActualizeSourcesHandler(ISourceActualizer sourceActualizer)
        {
            _sourceActualizer = sourceActualizer;
        }

        public async Task<Unit> Handle(ActualizeSources request, CancellationToken cancellationToken)
        {
            await _sourceActualizer.RequestActualization();

            return Unit.Value;
        }
    }
}