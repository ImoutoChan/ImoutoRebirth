using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Meido.Core;
using ImoutoRebirth.Meido.Core.ParsingStatus;
using MediatR;

namespace ImoutoRebirth.Meido.Services.Cqrs.Commands;

internal class SearchCompleteCommandHandler : ICommandHandler<SearchCompleteCommand>
{
    private readonly IParsingService _parsingService;

    public SearchCompleteCommandHandler(IParsingService parsingService)
    {
        _parsingService = parsingService;
    }

    public async Task<Unit> Handle(SearchCompleteCommand request, CancellationToken cancellationToken)
    {
        var source = (MetadataSource)request.SourceId; 
        
        await _parsingService.SaveSearchResult(
            request.SourceId, 
            request.FileId, 
            (SearchStatus)request.ResultStatus, 
            request.FileIdFromSource, 
            request.ErrorText);

        if (source == MetadataSource.Danbooru && (SearchStatus)request.ResultStatus == SearchStatus.NotFound)
        {
            await _parsingService.CreateGelbooruParsingStatus(request.FileId);
        }

        return Unit.Value;
    }
}
