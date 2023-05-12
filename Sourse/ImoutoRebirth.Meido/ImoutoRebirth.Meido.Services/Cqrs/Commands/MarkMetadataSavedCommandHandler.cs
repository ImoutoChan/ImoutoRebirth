using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Meido.Core.ParsingStatus;
using MediatR;

namespace ImoutoRebirth.Meido.Services.Cqrs.Commands;

internal class MarkMetadataSavedCommandHandler : ICommandHandler<MarkMetadataSavedCommand>
{
    private readonly IParsingService _parsingService;

    public MarkMetadataSavedCommandHandler(IParsingService parsingService)
    {
        _parsingService = parsingService;
    }

    public async Task<Unit> Handle(MarkMetadataSavedCommand request, CancellationToken cancellationToken)
    {
        await _parsingService.MarkAsSaved(request.FileId, request.SourceId);

        return Unit.Value;
    }
}