using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Room.Application.Services;

namespace ImoutoRebirth.Room.Application.Cqrs.FoldersSlice;

public record DeleteSourceFolderCommand(Guid CollectionId, Guid SourceFolderId) : ICommand;
    
internal class DeleteSourceFolderCommandHandler : ICommandHandler<DeleteSourceFolderCommand>
{
    private readonly ICollectionRepository _collectionRepository;
    private readonly IEventStorage _eventStorage;

    public DeleteSourceFolderCommandHandler(ICollectionRepository collectionFileRepository, IEventStorage eventStorage)
    {
        _collectionRepository = collectionFileRepository;
        _eventStorage = eventStorage;
    }

    public async Task Handle(DeleteSourceFolderCommand request, CancellationToken cancellationToken)
    {
        var (collectionId, sourceFolderId) = request;
        var collection = await _collectionRepository.GetById(collectionId).GetAggregateOrThrow();

        var result = collection.RemoveSourceFolder(sourceFolderId);

        _eventStorage.AddRange(result.EventsCollection);
        await _collectionRepository.Update(collection);
    }
}
