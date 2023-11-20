using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Room.Application.Services;

namespace ImoutoRebirth.Room.Application.Cqrs.CollectionSlice;

public record DeleteCollectionCommand(Guid Id) : ICommand;

internal class DeleteCollectionCommandHandler : ICommandHandler<DeleteCollectionCommand>
{
    private readonly ICollectionRepository _collectionRepository;

    public DeleteCollectionCommandHandler(ICollectionRepository collectionFileRepository) 
        => _collectionRepository = collectionFileRepository;

    public async Task Handle(DeleteCollectionCommand request, CancellationToken ct) 
        => await _collectionRepository.Remove(request.Id);
}
