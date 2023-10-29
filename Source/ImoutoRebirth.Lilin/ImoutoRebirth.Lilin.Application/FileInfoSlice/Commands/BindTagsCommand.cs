using System.Data;
using ImoutoRebirth.Common;
using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Common.Cqrs.Behaviors;
using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Lilin.Application.Persistence;
using ImoutoRebirth.Lilin.Domain.FileInfoAggregate;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Lilin.Application.FileInfoSlice.Commands;

[CommandQuery(IsolationLevel.ReadCommitted)]
public record BindTagsCommand(IReadOnlyCollection<BindTag> FileTags, SameTagHandleStrategy SameTagHandleStrategy) 
    : ICommand;

public record BindTag(Guid FileId, Guid TagId, string? Value, MetadataSource Source);

internal class BindTagsCommandHandler : ICommandHandler<BindTagsCommand>
{
    private readonly ILogger<BindTagsCommandHandler> _logger;
    private readonly IFileInfoRepository _fileInfoRepository;
    private readonly IEventStorage _eventStorage;

    public BindTagsCommandHandler(
        ILogger<BindTagsCommandHandler> logger,
        IFileInfoRepository fileInfoRepository,
        IEventStorage eventStorage)
    {
        _logger = logger;
        _fileInfoRepository = fileInfoRepository;
        _eventStorage = eventStorage;
    }

    public async Task Handle(BindTagsCommand command, CancellationToken ct)
    {
        var (bindTags, sameTagHandleStrategy) = command;

        if (bindTags.None())
        {
            _logger.LogWarning("BindTagsCommand was called with empty tags");
            return;
        }

        var groupedFileTags = bindTags
            .Select(
                x => new FileTag(
                    x.FileId,
                    x.TagId,
                    x.Value,
                    x.Source))
            .GroupBy(x => x.FileId)
            .ToArray();

        foreach (var fileTags in groupedFileTags)
        {
            var fileInfo = await _fileInfoRepository.Get(fileTags.Key, ct);
            
            var domainResult = fileInfo.UpdateTags(fileTags.ToList(), sameTagHandleStrategy);
            await _fileInfoRepository.Save(fileInfo);
            _eventStorage.AddRange(domainResult);
        }
    }
}
