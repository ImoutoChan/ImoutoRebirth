using ImoutoRebirth.Common;
using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Room.Application.Services;
using ImoutoRebirth.Room.Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Room.Application.Cqrs.CollectionFileSlice;

public record DeleteCollectionFileCommand(Guid Id) : ICommand;

internal class DeleteCollectionFileCommandHandler : ICommandHandler<DeleteCollectionFileCommand>
{
    private const string DeletedDirectoryName = "!Deleted";

    private readonly ICollectionFileRepository _collectionFileRepository;
    private readonly ILogger<DeleteCollectionFileCommandHandler> _logger;
    private readonly ICollectionRepository _collectionRepository;
    private readonly IMediator _mediator;

    public DeleteCollectionFileCommandHandler(
        ICollectionFileRepository collectionFileRepository,
        ILogger<DeleteCollectionFileCommandHandler> logger,
        ICollectionRepository collectionRepository,
        IMediator mediator)
    {
        _collectionFileRepository = collectionFileRepository;
        _logger = logger;
        _collectionRepository = collectionRepository;
        _mediator = mediator;
    }

    public async Task Handle(DeleteCollectionFileCommand request, CancellationToken ct)
    {
        var id = request.Id;
        
        var found = await _mediator.Send(new CollectionFilesModelsQuery(
            new CollectionFilesQuery(default, new[] {id}, default, Array.Empty<string>(), default, default)), ct);

        if (found.None())
            return;

        foreach (var file in found) 
            await DeleteCollectionFile(file);

        await _collectionFileRepository.Remove(id);
    }

    private async Task DeleteCollectionFile(CollectionFile file)
    {
        using var scope = _logger.BeginScope("Deleting file {File} {Md5}", file.Path, file.Md5);

        var fileToDelete = new FileInfo(file.Path);
        var deletedDirectory = await GetDeletedFolder(fileToDelete, file.CollectionId);

        if (deletedDirectory != null)
        {
            var fileToDeleteDestination = new FileInfo(Path.Combine(deletedDirectory.FullName, fileToDelete.Name));
            new SystemFile(fileToDelete, file.Md5, file.Size).MoveFile(ref fileToDeleteDestination);
        }
        else
        {
            new SystemFile(fileToDelete, file.Md5, file.Size).Delete();
        }
    }

    private async Task<DirectoryInfo?> GetDeletedFolder(FileInfo fileToDelete, Guid collectionId)
    {
        var collection = await _collectionRepository.GetById(collectionId).GetAggregateOrThrow();
        var destinationDirectory = collection.DestinationFolder.DestinationDirectory;

        var fileDirectory = fileToDelete.Directory!;

        if (destinationDirectory == null)
        {
            // In collection without destination it's unwise to move files around.
            // They will be just added again. So the only way is to delete them.
            _logger.LogInformation("Destination directory isn't found");
            return null;
        }

        var filePathLowerCase = fileDirectory.FullName.ToLowerInvariant();
        var destPathLowerCase = destinationDirectory.FullName.ToLowerInvariant();

        var isFileInDestinationDirectory = filePathLowerCase.Contains(destPathLowerCase);

        if (isFileInDestinationDirectory)
        {
            return destinationDirectory.CreateSubdirectory(DeletedDirectoryName);
        }

        _logger.LogWarning(
            "Destination directory {DestinationDirectory} doesn't match with file {File}",
            destPathLowerCase,
            filePathLowerCase);

        return fileDirectory.CreateSubdirectory(DeletedDirectoryName);
    }
}
