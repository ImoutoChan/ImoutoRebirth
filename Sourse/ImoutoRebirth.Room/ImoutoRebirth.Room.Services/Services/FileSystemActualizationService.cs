using ImoutoRebirth.Common;
using ImoutoRebirth.Room.Core.Models;
using ImoutoRebirth.Room.Core.Services.Abstract;
using ImoutoRebirth.Room.DataAccess;
using ImoutoRebirth.Room.DataAccess.Models;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Room.Core.Services;

public class FileSystemActualizationService : IFileSystemActualizationService
{
    private readonly ISourceFolderService _sourceFolderService;
    private readonly IDestinationFolderService _destinationFolderService;
    private readonly ICollectionFileService _collectionFileService;
    private readonly IDbStateService _dbStateService;
    private readonly IRemoteCommandService _remoteCommandService;
    private readonly IImoutoPicsUploader _imoutoPicsUploader;
    private readonly ILogger<FileSystemActualizationService> _logger;

    public FileSystemActualizationService(
        ISourceFolderService sourceFolderService,
        IDestinationFolderService destinationFolderService,
        ICollectionFileService collectionFileService,
        IDbStateService dbStateService,
        IRemoteCommandService remoteCommandService,
        ILogger<FileSystemActualizationService> logger,
        IImoutoPicsUploader imoutoPicsUploader)
    {
        _sourceFolderService = sourceFolderService;
        _destinationFolderService = destinationFolderService;
        _collectionFileService = collectionFileService;
        _dbStateService = dbStateService;
        _remoteCommandService = remoteCommandService;
        _logger = logger;
        _imoutoPicsUploader = imoutoPicsUploader;
    }

    public async Task PryCollection(OversawCollection oversawCollection)
    {
        _logger.LogTrace("Prying collection {CollectionName}", oversawCollection.Collection.Name);

        foreach (var collectionSourceFolder in oversawCollection.SourceFolders)
        {
            await ProcessSourceFolder(oversawCollection, collectionSourceFolder);
        }
    }

    private async Task ProcessSourceFolder(
        OversawCollection oversawCollection, 
        SourceFolder collectionSourceFolder)
    {
        using var loggerScope = _logger.BeginScope(
            "Looking at {SourceFolderPath}...",
            collectionSourceFolder.Path);

        var newFiles = await _sourceFolderService.GetNewFiles(collectionSourceFolder);

        if (!newFiles.Any())
        {
            return;
        }

        _logger.LogInformation("{NewFilesCount} new files found", newFiles.Count);

        var movedFiles = await MoveFiles(newFiles, oversawCollection);
            
        await _dbStateService.SaveChanges();

        _logger.LogInformation("{NewFilesSavedCount} files saved", movedFiles.Count);


        await movedFiles.Select(x => _remoteCommandService.SaveTags(x.FileId, x.MovedInformation.SourceTags))
            .WhenAll();

        await movedFiles
            .Select(
                x => _remoteCommandService.UpdateMetadataRequest(x.FileId, x.MovedInformation.SystemFile.Md5))
            .WhenAll();
        
        await movedFiles
            .Select(x => _imoutoPicsUploader.UploadFile(x.MovedInformation.SystemFile.File.FullName))
            .WhenAll();

        _logger.LogDebug("Update metadata requests are sent");
    }

    private async Task<IReadOnlyCollection<(Guid FileId, MovedInformation MovedInformation)>> MoveFiles(
        IEnumerable<MoveInformation> preparedFiles,
        OversawCollection oversawCollection)
    {
        var result = new List<(Guid FileId, MovedInformation movedInformation)>();

        foreach (var moveInformation in preparedFiles)
        {
            var movedInformation = MoveFile(oversawCollection, moveInformation);

            if (!movedInformation.RequireSave)
                continue;

            var fileId = await _collectionFileService.SaveNew(movedInformation, oversawCollection.Collection.Id);

            result.Add((fileId, movedInformation));
        }

        return result;
    }

    private MovedInformation MoveFile(OversawCollection oversawCollection, MoveInformation moveInformation)
    {
        using var loggerScope = _logger.BeginScope(
            "Processing new file in source: {NewFile}, {MoveProblem}, {@SourceTags}",
            moveInformation.SystemFile.File.FullName,
            moveInformation.MoveProblem,
            moveInformation.SourceTags);

        try
        {
            return _destinationFolderService.Move(oversawCollection.DestinationFolder, moveInformation);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred while moving file");
            return new MovedInformation(moveInformation, false, moveInformation.SystemFile.File);
        }
    }
}
