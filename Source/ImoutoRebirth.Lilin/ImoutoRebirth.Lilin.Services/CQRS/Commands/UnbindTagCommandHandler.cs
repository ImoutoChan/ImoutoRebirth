using System.Data;
using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Common.Cqrs.Behaviors;
using ImoutoRebirth.Lilin.Core.Models;
using ImoutoRebirth.Lilin.Services.ApplicationServices;

namespace ImoutoRebirth.Lilin.Services.CQRS.Commands;

[CommandQuery(IsolationLevel.ReadCommitted)]
public record UnbindTagCommand(FileTagInfo FileTag) : ICommand;

internal class UnbindTagCommandHandler : ICommandHandler<UnbindTagCommand>
{
    private readonly IFileInfoService _fileInfoService;

    public UnbindTagCommandHandler(IFileInfoService fileInfoService) => _fileInfoService = fileInfoService;

    public async Task Handle(UnbindTagCommand request, CancellationToken ct)
    {
        var fileInfo = await _fileInfoService.LoadFileAggregate(request.FileTag.FileId, ct);

        fileInfo.RemoveFileTag(request.FileTag.TagId, request.FileTag.Source, request.FileTag.Value);

        await _fileInfoService.PersistFileAggregate(fileInfo);
    }
}
