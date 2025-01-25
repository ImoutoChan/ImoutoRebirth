using System.Runtime.CompilerServices;
using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Room.Application.Cqrs.ImoutoPicsUploadStateSlice;
using ImoutoRebirth.Room.Application.Services;
using ImoutoRebirth.Room.Domain;
using ImoutoRebirth.Room.Domain.CollectionAggregate;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Room.Application.Cqrs;

public record OverseeCollectionCommand(Guid CollectionId) : ICommand<OverseeCollectionResult>;

public record OverseeCollectionResult(bool AnyFileMoved);

internal class OverseeCollectionCommandHandler : ICommandHandler<OverseeCollectionCommand, OverseeCollectionResult>
{
    private readonly ICollectionRepository _collectionRepository;
    private readonly ICollectionFileRepository _collectionFileRepository;
    private readonly ILogger<OverseeCollectionCommandHandler> _logger;
    private readonly IImageService _imageService;
    private readonly IRemoteCommandService _remoteCommandService;
    private readonly IImoutoPicsUploader _imoutoPicsUploader;
    private readonly IMediator _mediator;

    public OverseeCollectionCommandHandler(
        ICollectionRepository collectionRepository,
        ICollectionFileRepository collectionFileRepository,
        ILogger<OverseeCollectionCommandHandler> logger,
        IImageService imageService,
        IRemoteCommandService remoteCommandService,
        IImoutoPicsUploader imoutoPicsUploader,
        IMediator mediator)
    {
        _collectionRepository = collectionRepository;
        _collectionFileRepository = collectionFileRepository;
        _logger = logger;
        _imageService = imageService;
        _remoteCommandService = remoteCommandService;
        _imoutoPicsUploader = imoutoPicsUploader;
        _mediator = mediator;
    }

    public async Task<OverseeCollectionResult> Handle(OverseeCollectionCommand command, CancellationToken ct)
    {
        var collectionId = command.CollectionId;
        var collection = await _collectionRepository.GetById(collectionId).GetAggregateOrThrow();

        var anyFileAdded = false;
        foreach (var sourceFolder in collection.SourceFolders)
        {
            var prepared = PrepareNewFiles(collectionId, sourceFolder, ct);
            var withoutDuplicates = RemoveMd5Duplicates(prepared);
            
            await foreach (var preparedToMove in withoutDuplicates)
            {
                var moved = MoveFile(collection, preparedToMove);

                if (!moved.RequireSave) 
                    continue;

                await SaveAndReport(collectionId, moved);
                anyFileAdded = true;
            }
        }

        return new(anyFileAdded);
    }
    
    private async Task SaveAndReport(Guid collectionId, SystemFileMoved moved)
    {
        var newId = await _mediator.Send(new SaveNewFileCommand(collectionId, moved));

        await _remoteCommandService.SaveTags(newId, moved.SourceTags);
        await _remoteCommandService.UpdateMetadataRequest(newId, moved.SystemFile.Md5);
        
        var isUploadEnabled = await _mediator.Send(new IsImoutoPicsUploaderEnabledQuery());
        
        if (isUploadEnabled)
            await _imoutoPicsUploader.UploadFile(moved.MovedFileInfo.FullName);
    }

    private SystemFileMoved MoveFile(Collection collection, SystemFilePreparedToMove preparedToMove)
    {
        using var loggerScope = _logger.BeginScope(
            "Processing new file in source: {NewFile}, {MoveProblem}, {@SourceTags}",
            preparedToMove.SystemFile.File.FullName,
            preparedToMove.MoveProblem,
            preparedToMove.SourceTags);

        try
        {
            return collection.DestinationFolder.Move(preparedToMove);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred while moving file");
            return preparedToMove.CreateMovedFail(e.Message);
        }
    }

    private async IAsyncEnumerable<SystemFilePreparedToMove> RemoveMd5Duplicates(
        IAsyncEnumerable<SystemFilePreparedToMove> prepared)
    {
        var processed = new List<SystemFilePreparedToMove>();

        await foreach (var file in prepared)
        {
            var alreadyProcessed = processed.FirstOrDefault(x => x.SystemFile.Md5 == file.SystemFile.Md5);
            if (alreadyProcessed != null)
            {
                _logger.LogWarning(
                    "Duplicate files in source folder ignored: {File}, {DuplicateFile}",
                    alreadyProcessed.SystemFile.File.FullName,
                    file.SystemFile.File.FullName);
                continue;
            }
            processed.Add(file);
            yield return file;
        }
    }

    private async IAsyncEnumerable<SystemFilePreparedToMove> PrepareNewFiles(
        Guid collectionId,
        SourceFolder sourceFolder,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var files = sourceFolder.GetAllFileInfo().ToList();
        var filteredFiles = await FilterOutExistingFiles(collectionId, files, ct);

        foreach (var file in filteredFiles)
        {
            var systemFile = SystemFile.Create(file);

            if (systemFile == null)
            {
                _logger.LogWarning("File isn't ready or was removed");
                continue;
            }

            var existingFile = await _collectionFileRepository.GetWithMd5(collectionId, systemFile.Md5, ct);

            var isCorrectIfImage
                = !_imageService.IsImageHasSupportedExtension(systemFile)
                  || _imageService.IsImageCorrect(systemFile);

            var preparedToMove = sourceFolder.PrepareFileToMove(
                systemFile,
                existingFile != null,
                isCorrectIfImage);

            yield return preparedToMove;
        }
    }

    private async Task<IReadOnlyCollection<FileInfo>> FilterOutExistingFiles(
        Guid collectionId,
        IReadOnlyCollection<FileInfo> files,
        CancellationToken ct = default)
    {
        var filePaths = files.Select(x => x.FullName).ToList();
        var newPaths = await _collectionFileRepository.FilterOutExistingPaths(collectionId, filePaths, ct);
        newPaths = newPaths.ToHashSet();
        return files.Where(x => newPaths.Contains(x.FullName)).ToList();
    }
}
