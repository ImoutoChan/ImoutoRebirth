using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Room.Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Room.Application.Cqrs;
    
/// <summary>
/// Updates location tags of all files in collections, where:
///     1. Destination Folder is null;
///     2. Source Folder is marked with ShouldAddTagFromFilename flag.
/// </summary>
public record UpdateLocationTagsCommand : ICommand;

internal class UpdateLocationTagsCommandHandler : ICommandHandler<UpdateLocationTagsCommand>
{
    private readonly ICollectionRepository _collectionRepository;
    private readonly ILogger<UpdateLocationTagsCommandHandler> _logger;
    private readonly ICollectionFileRepository _collectionFileRepository;
    private readonly IRemoteCommandService _remoteCommandService;
    private readonly IMediator _mediator;

    public UpdateLocationTagsCommandHandler(
        ICollectionRepository collectionRepository,
        ILogger<UpdateLocationTagsCommandHandler> logger,
        ICollectionFileRepository collectionFileRepository,
        IRemoteCommandService remoteCommandService,
        IMediator mediator)
    {
        _collectionRepository = collectionRepository;
        _logger = logger;
        _collectionFileRepository = collectionFileRepository;
        _remoteCommandService = remoteCommandService;
        _mediator = mediator;
    }

    public async Task Handle(UpdateLocationTagsCommand request, CancellationToken ct)
    {
        try
        {
            var collections = await _collectionRepository.GetAll();

            foreach (var collection in collections)
            {
                if (collection.DestinationFolder.DestinationDirectory != null)
                    continue;

                foreach (var sourceFolder in collection.SourceFolders)
                {
                    if (sourceFolder is { ShouldAddTagFromFilename: false, ShouldCreateTagsFromSubfolders: false })
                        continue;

                    await UpdateLocationTagsInSourceFolder(sourceFolder);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while updating local tags");
        }
    }

    private async Task UpdateLocationTagsInSourceFolder(SourceFolder sourceFolder)
    {
        var allFiles = sourceFolder.GetAllFileInfo().ToList();

        _logger.LogInformation(
            "Updating location tags for {SourceFolder}, found {Count} new files",
            sourceFolder.Path,
            allFiles.Count);
            
        foreach (var file in allFiles)
        {
            var foundFiles = await _mediator.Send(
                new CollectionFilesModelsQuery(new CollectionFilesQuery(
                    CollectionId: default,
                    CollectionFileIds: Array.Empty<Guid>(),
                    Path: file.FullName,
                    Md5: Array.Empty<string>(),
                    Count: 1,
                    Skip: 0)));
                
            var foundFile = foundFiles.FirstOrDefault();

            if (foundFile == null)
            {
                _logger.LogWarning("File {FilePath} was not found in collection", file.FullName);
                continue;
            }

            var systemFile = SystemFile.Create(file);
            if (systemFile == null)
            {
                _logger.LogWarning("File {FilePath} is not available right now", file.FullName);
                continue;
            }
            
            IEnumerable<string> tags = Array.Empty<string>();

            if (sourceFolder.ShouldAddTagFromFilename)
                tags = tags.Union(systemFile.GetTagsFromName());
                
            if (sourceFolder.ShouldCreateTagsFromSubfolders)
                tags = tags.Union(systemFile.GetTagsFromPathForSourceFolder(sourceFolder));

            var readyTags = tags.ToList();
                
            _logger.LogInformation(
                "Saving location tags for {FileId} {FilePath} {Tags}",
                foundFile.Id,
                foundFile.Path,
                readyTags);
                
            await _remoteCommandService.SaveTags(foundFile.Id, readyTags);
        }
    }
}
