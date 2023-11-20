using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Room.Application.Services;

namespace ImoutoRebirth.Room.Application.Cqrs.FoldersSlice;

public record DeleteDestinationFolderCommand(Guid CollectionId) : ICommand;

internal class DeleteDestinationFolderCommandHandler : ICommandHandler<DeleteDestinationFolderCommand>
{
    private readonly ICollectionRepository _collectionRepository;

    public DeleteDestinationFolderCommandHandler(ICollectionRepository collectionFileRepository) 
        => _collectionRepository = collectionFileRepository;

    public async Task Handle(DeleteDestinationFolderCommand request, CancellationToken cancellationToken)
    {
        var collection = await _collectionRepository.GetById(request.CollectionId).GetAggregateOrThrow();

        collection.DeleteDestinationFolder();

        await _collectionRepository.Update(collection);
    }
}
