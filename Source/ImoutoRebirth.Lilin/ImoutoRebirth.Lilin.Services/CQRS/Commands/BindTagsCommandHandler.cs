using System.Data;
using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Common.Cqrs.Behaviors;
using ImoutoRebirth.Lilin.Core.Infrastructure;
using ImoutoRebirth.Lilin.Core.Models;
using ImoutoRebirth.Lilin.Core.Models.FileInfoAggregate;
using ImoutoRebirth.Lilin.Services.ApplicationServices;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Lilin.Services.CQRS.Commands;

[CommandQuery(IsolationLevel.ReadCommitted)]
public record BindTagsCommand(IReadOnlyCollection<FileTagInfo> FileTags, SameTagHandleStrategy SameTagHandleStrategy) 
    : ICommand;

internal class BindTagsCommandHandler : ICommandHandler<BindTagsCommand>
{
    private readonly ILogger<BindTagsCommandHandler> _logger;
    private readonly IFileInfoService _fileInfoService;
    private readonly ITagRepository _tagRepository;

    public BindTagsCommandHandler(
        ILogger<BindTagsCommandHandler> logger,
        IFileInfoService fileInfoService,
        ITagRepository tagRepository)
    {
        _logger = logger;
        _fileInfoService = fileInfoService;
        _tagRepository = tagRepository;
    }

    public async Task Handle(BindTagsCommand request, CancellationToken ct)
    {
        if (!request.FileTags.Any())
        {
            _logger.LogWarning("Trying to batch add {FileTagCount} new FileTags", request.FileTags.Count);
            return;
        }

        var tags = await LoadTags(request, ct);
        var fileInfos = await LoadFileInfos(request, ct);

        var newFileTags = request
            .FileTags
            .Select(
                x => new FileTag(
                    x.FileId,
                    tags[x.TagId],
                    x.Value,
                    x.Source))
            .ToArray();
            
        var fileInfoPack = new FileInfoPack(fileInfos);
        fileInfoPack.UpdateTags(newFileTags, request.SameTagHandleStrategy);

        foreach (var file in fileInfoPack.Files)
        {
            await _fileInfoService.PersistFileAggregate(file);
        }

        _logger.LogInformation("Batch added {FileTagCount} new FileTags", request.FileTags.Count);
    }

    private async Task<IReadOnlyCollection<FileInfo>> LoadFileInfos(BindTagsCommand request, CancellationToken ct)
    {
        var fileIds = request
            .FileTags
            .Select(x => x.FileId)
            .Distinct()
            .ToArray();

        var fileInfos = new List<FileInfo>(fileIds.Length);
        foreach (var fileId in fileIds)
        {
            var fileInfo = await _fileInfoService.LoadFileAggregate(fileId, ct);
            fileInfos.Add(fileInfo);
        }
            
        return fileInfos;
    }

    private async Task<Dictionary<Guid, Tag>> LoadTags(BindTagsCommand request, CancellationToken ct)
    {
        var tagIds = request
            .FileTags
            .Select(x => x.TagId)
            .Distinct()
            .ToArray();

        var tags = new Dictionary<Guid, Tag>(tagIds.Length);
        foreach (var tagId in tagIds)
        {
            var tag = await _tagRepository.Get(tagId, ct);
            if (tag == null)
                throw new ApplicationException($"Tag with id {tagId} is not found");

            tags.Add(tagId, tag);
        }

        return tags;
    }
}
