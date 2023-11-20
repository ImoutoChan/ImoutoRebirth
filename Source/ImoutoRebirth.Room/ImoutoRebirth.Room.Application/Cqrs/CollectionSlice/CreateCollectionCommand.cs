using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Room.Application.Services;
using ImoutoRebirth.Room.Domain.CollectionAggregate;

namespace ImoutoRebirth.Room.Application.Cqrs.CollectionSlice;

public record CreateCollectionCommand(string Name) : ICommand;

internal class CreateCollectionCommandHandler : ICommandHandler<CreateCollectionCommand>
{
    private readonly ICollectionRepository _collectionRepository;

    public CreateCollectionCommandHandler(ICollectionRepository collectionFileRepository) 
        => _collectionRepository = collectionFileRepository;

    public async Task Handle(CreateCollectionCommand request, CancellationToken cancellationToken)
    {
        var name = request.Name;
        var collection = Collection.Create(name);

        await _collectionRepository.Create(collection);
    }
}
