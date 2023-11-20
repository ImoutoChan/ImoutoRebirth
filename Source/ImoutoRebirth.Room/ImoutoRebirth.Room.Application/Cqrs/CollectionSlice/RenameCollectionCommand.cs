using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Room.Application.Services;

namespace ImoutoRebirth.Room.Application.Cqrs.CollectionSlice;

public record RenameCollectionCommand(Guid Id, string NewName) : ICommand;

internal class RenameCollectionCommandHandler : ICommandHandler<RenameCollectionCommand>
{
    private readonly ICollectionRepository _collectionRepository;

    public RenameCollectionCommandHandler(ICollectionRepository collectionFileRepository) 
        => _collectionRepository = collectionFileRepository;

    public async Task Handle(RenameCollectionCommand request, CancellationToken cancellationToken)
    {
        var (id, newName) = request;

        var collection = await _collectionRepository.GetById(id).GetAggregateOrThrow();

        collection.Rename(newName);
        
        await _collectionRepository.Update(collection);
    }
}
