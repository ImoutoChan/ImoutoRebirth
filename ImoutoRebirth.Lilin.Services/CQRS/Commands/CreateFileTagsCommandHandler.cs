using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Core.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Lilin.Services.CQRS.Commands
{
    public class CreateFileTagsCommandHandler : ICommandHandler<CreateFileTagsCommand>
    {
        private readonly IFileTagRepository _fileTagRepository;
        private readonly ILogger<CreateFileTagsCommandHandler> _logger;

        public CreateFileTagsCommandHandler(
            IFileTagRepository fileTagRepository, 
            ILogger<CreateFileTagsCommandHandler> logger)
        {
            _fileTagRepository = fileTagRepository;
            _logger = logger;
        }

        public async Task<Unit> Handle(CreateFileTagsCommand request, CancellationToken cancellationToken)
        {
            if (request.FileTags.Any())
            {
                _logger.LogWarning("Trying to batch add {FileTagCount} new FileTags", 0);
                return Unit.Value;
            }

            await _fileTagRepository.AddBatch(request.FileTags);
            _logger.LogInformation("Batch added {FileTagCount} new FileTags", request.FileTags.Count);
            return Unit.Value;
        }
    }
}