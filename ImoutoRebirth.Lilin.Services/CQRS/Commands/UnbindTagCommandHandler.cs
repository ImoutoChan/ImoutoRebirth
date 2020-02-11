using System.Threading;
using System.Threading.Tasks;
using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Services.ApplicationServices;
using MediatR;

namespace ImoutoRebirth.Lilin.Services.CQRS.Commands
{
    public class UnbindTagCommandHandler : ICommandHandler<UnbindTagCommand>
    {
        private readonly IFileInfoService _fileInfoService;

        public UnbindTagCommandHandler(IFileInfoService fileInfoService)
        {
            _fileInfoService = fileInfoService;
        }

        public async Task<Unit> Handle(UnbindTagCommand request, CancellationToken cancellationToken)
        {
            var fileInfo = await _fileInfoService.LoadFileAggregate(request.FileTag.FileId);

            fileInfo.RemoveFileTag(request.FileTag.TagId, request.FileTag.Source, request.FileTag.Value);

            await _fileInfoService.PersistFileAggregate(fileInfo);
            return Unit.Value;
        }
    }
}