using System.Data;
using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Common.Cqrs.Behaviors;
using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Lilin.Application.Persistence;

namespace ImoutoRebirth.Lilin.Application.FileInfoSlice.Commands;

[CommandQuery(IsolationLevel.ReadCommitted)]
public record UnbindTagsCommand(IReadOnlyCollection<BindTag> FileTags) : ICommand;

internal class UnbindTagsCommandHandler : ICommandHandler<UnbindTagsCommand>
{
    private readonly IFileInfoRepository _fileInfoRepository;
    private readonly IEventStorage _eventStorage;

    public UnbindTagsCommandHandler(IFileInfoRepository fileInfoRepository, IEventStorage eventStorage)
    {
        _fileInfoRepository = fileInfoRepository;
        _eventStorage = eventStorage;
    }

    public async Task Handle(UnbindTagsCommand request, CancellationToken ct)
    {
        foreach (var fileTag in request.FileTags)
        {
            var fileInfo = await _fileInfoRepository.Get(fileTag.FileId, ct);

            var domainResult = fileInfo.RemoveFileTag(fileTag.TagId, fileTag.Source, fileTag.Value);

            await _fileInfoRepository.Save(fileInfo);
            _eventStorage.AddRange(domainResult);
        }
    }
}
