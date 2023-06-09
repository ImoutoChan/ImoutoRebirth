using System.Data;
using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Common.Cqrs.Behaviors;
using ImoutoRebirth.Lilin.Core.Models;
using ImoutoRebirth.Lilin.Services.ApplicationServices;

namespace ImoutoRebirth.Lilin.Services.CQRS.Commands;

[CommandQuery(IsolationLevel.ReadCommitted)]
public record UnbindTagsCommand(IReadOnlyCollection<FileTagInfo> FileTags) : ICommand;

internal class UnbindTagsCommandHandler : ICommandHandler<UnbindTagsCommand>
{
    private readonly IFileInfoService _fileInfoService;

    public UnbindTagsCommandHandler(IFileInfoService fileInfoService) => _fileInfoService = fileInfoService;

    public async Task Handle(UnbindTagsCommand request, CancellationToken ct)
    {
        foreach (var fileTag in request.FileTags)
        {
            var fileInfo = await _fileInfoService.LoadFileAggregate(fileTag.FileId, ct);

            fileInfo.RemoveFileTag(fileTag.TagId, fileTag.Source, fileTag.Value);

            await _fileInfoService.PersistFileAggregate(fileInfo);
        }
    }
}
