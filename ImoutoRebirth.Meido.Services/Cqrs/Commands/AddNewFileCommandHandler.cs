using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Meido.Core.ParsingStatus;
using MediatR;

namespace ImoutoRebirth.Meido.Services.Cqrs.Commands;

internal class AddNewFileCommandHandler : ICommandHandler<AddNewFileCommand>
{
    private readonly IParsingService _parsingService;

    public AddNewFileCommandHandler(IParsingService parsingService)
    {
        _parsingService = parsingService;
    }

    public async Task<Unit> Handle(AddNewFileCommand request, CancellationToken cancellationToken)
    {
        await _parsingService.CreateParsingStatusesForNewFile(request.FileId, request.Md5);

        return Unit.Value;
    }
}