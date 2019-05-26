using System.Threading;
using System.Threading.Tasks;
using ImoutoProject.Common.Cqrs.Abstract;
using ImoutoRebirth.Meido.Core.ParsingStatus;
using MediatR;

namespace ImoutoRebirth.Meido.Services.Cqrs.Commands
{
    internal class SearchCompleteCommandHandler : ICommandHandler<SearchCompleteCommand>
    {
        private readonly IParsingService _parsingService;

        public SearchCompleteCommandHandler(IParsingService parsingService)
        {
            _parsingService = parsingService;
        }

        public async Task<Unit> Handle(SearchCompleteCommand request, CancellationToken cancellationToken)
        {
            await _parsingService.SaveSearchResult(
                request.SourceId, 
                request.FileId, 
                (SearchStatus)request.ResultStatus, 
                request.FileIdFromSource, 
                request.ErrorText);

            return Unit.Value;
        }
    }
}