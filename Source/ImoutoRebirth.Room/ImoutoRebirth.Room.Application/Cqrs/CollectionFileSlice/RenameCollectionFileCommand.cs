using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Common;
using ImoutoRebirth.Room.Application.Services;
using ImoutoRebirth.Room.Domain;
using ImoutoRebirth.Room.Domain.CollectionAggregate;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Room.Application.Cqrs.CollectionFileSlice;

public record RenameCollectionFileCommand(Guid Id, string NewFileName) : ICommand<string>;

public record CanRenameCollectionFileToCommand(Guid Id, string NewFileName) : ICommand<CanRenameCollectionFileToResult>;

public record CanRenameCollectionFileToResult(bool CanRename, string? ReasonWhyNot);

internal class RenameCollectionFileCommandHandler 
    : ICommandHandler<RenameCollectionFileCommand, string>
    , ICommandHandler<CanRenameCollectionFileToCommand, CanRenameCollectionFileToResult>
{
    private readonly ICollectionFileRepository _collectionFileRepository;
    private readonly ILogger<RenameCollectionFileCommandHandler> _logger;
    private readonly ICollectionRepository _collectionRepository;
    private readonly IRemoteCommandService _remoteCommandService;

    public RenameCollectionFileCommandHandler(
        ICollectionFileRepository collectionFileRepository,
        ILogger<RenameCollectionFileCommandHandler> logger,
        ICollectionRepository collectionRepository,
        IRemoteCommandService remoteCommandService)
    {
        _collectionFileRepository = collectionFileRepository;
        _logger = logger;
        _collectionRepository = collectionRepository;
        _remoteCommandService = remoteCommandService;
    }

    public async Task<string> Handle(RenameCollectionFileCommand request, CancellationToken ct)
    {
        var (id, newFileName) = request;

        var file = await _collectionFileRepository
            .GetById(id)
            .GetAggregateOrThrow(id);

        var collection = await _collectionRepository
            .GetById(file.CollectionId)
            .GetAggregateOrThrow(file.CollectionId);

        var canRename = collection.DestinationFolder.AllowManualFileRenaming();
        if (!canRename)
            throw new DomainException("Renaming files is not allowed when ShouldRenameByHash=true.");
        
        var oldFullName = file.Path;
        file.Rename(newFileName);
        var newFullName = file.Path;

        if (!File.Exists(oldFullName))
            throw new DomainException($"File '{oldFullName}' does not exist.");

        if (File.Exists(newFullName))
            throw new DomainException($"File '{newFullName}' already exists.");

        await _collectionFileRepository.Update(file);
        File.Move(oldFullName, file.Path);

        _logger.LogInformation(
            "Renamed file {FileId} from {OldFileName} to {NewFileName}",
            id,
            file.Path,
            newFileName);

        await UpdateLocationTags(collection, file);
        await _remoteCommandService.RequestMetadataUpdateForRenamedFile(file.Id, file.Md5, newFileName);
        await _remoteCommandService.UpdateFileMetadataRequest(file.Id, file.Path);

        return newFullName;
    }

    private async Task UpdateLocationTags(Collection collection, CollectionFile file)
    {
        if (collection.DestinationFolder.DestinationDirectory != null)
            return;

        var sourceFolder = collection.SourceFolders.FirstOrDefault(x => file.Path.StartsWith(x.Path));

        if (sourceFolder is { ShouldAddTagFromFilename: false, ShouldCreateTagsFromSubfolders: false } or null)
        {
            return;
        }

        await UpdateLocationTags(sourceFolder, file);
    }

    private async Task UpdateLocationTags(SourceFolder sourceFolder, CollectionFile file)
    {
        var systemFile = SystemFile.Create(new(file.Path));
        if (systemFile == null)
        {
            _logger.LogWarning("File {FilePath} is not available right now", file.Path);
            return;
        }

        var tags = new List<string>();

        if (sourceFolder.ShouldAddTagFromFilename)
            tags.AddRange(systemFile.GetTagsFromName());

        if (sourceFolder.ShouldCreateTagsFromSubfolders)
            tags.AddRange(systemFile.GetTagsFromPathForSourceFolder(sourceFolder));

        var readyTags = tags.ToList();

        _logger.LogInformation(
            "Saving location tags for {FileId} {FilePath} {Tags}",
            file.Id,
            file.Path,
            readyTags.JoinStrings(", "));

        await _remoteCommandService.SaveLocationTags(file.Id, readyTags);
    }

    public async Task<CanRenameCollectionFileToResult> Handle(CanRenameCollectionFileToCommand request, CancellationToken ct)
    {
        var (id, newFileName) = request;

        var file = await _collectionFileRepository
            .GetById(id)
            .GetAggregateOrThrow(id);

        var collection = await _collectionRepository
            .GetById(file.CollectionId)
            .GetAggregateOrThrow(file.CollectionId);

        var canRename = collection.DestinationFolder.AllowManualFileRenaming();
        if (!canRename)
        {
            return
                new (false, "The file collection has destination folder with auto renaming feature (ShouldRenameByHash) enabled. "
                + "You can't rename files in the collections configured that way");
        }

        if (newFileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            return
                new(false, $"The filename {newFileName} contains characters that shouldn't be used in name: "
                           + $"{new string(newFileName.Where(x => Path.GetInvalidFileNameChars().Contains(x)).ToArray())}");
        }

        var oldFullName = file.Path;
        file.Rename(newFileName);
        var newFullName = file.Path;

        if (!File.Exists(oldFullName))
        {
            return new(false, $"The file '{oldFullName}' seems to be missing, please make sure that the collection file is available");
        }

        if (File.Exists(newFullName))
        {
            return new(false, $"The file '{newFullName}' already exists, please select a different name");
        }

        return new(true, null);
    }
}
