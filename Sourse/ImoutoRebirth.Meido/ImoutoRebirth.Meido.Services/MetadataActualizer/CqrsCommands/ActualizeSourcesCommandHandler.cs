using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Meido.Core.SourceActualizingState;
using MediatR;
using Microsoft.Extensions.Options;

namespace ImoutoRebirth.Meido.Services.MetadataActualizer.CqrsCommands;

internal class ActualizeSourcesCommandHandler : ICommandHandler<ActualizeSourcesCommand>
{
    private readonly ISourceActualizerService _sourceActualizerService;
    private readonly MetadataActualizerSettings _actualizerSettings;

    public ActualizeSourcesCommandHandler(
        ISourceActualizerService sourceActualizerService,
        IOptionsSnapshot<MetadataActualizerSettings> actualizerSettings)
    {
        _sourceActualizerService = sourceActualizerService;
        _actualizerSettings = actualizerSettings.Value;
    }

    public async Task<Unit> Handle(
        ActualizeSourcesCommand request, 
        CancellationToken cancellationToken)
    {
        await _sourceActualizerService.RequestActualization(_actualizerSettings.ActiveSources);

        return Unit.Value;
    }
}