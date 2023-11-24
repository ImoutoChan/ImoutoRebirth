using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Room.Application.Services;
using MediatR;

namespace ImoutoRebirth.Room.Application.Cqrs;

public record OverseeCommand : ICommand<OverseeCollectionResult>;

internal class OverseeCommandHandler : ICommandHandler<OverseeCommand, OverseeCollectionResult>
{
    private readonly IMediator _mediator;
    private readonly ICollectionRepository _collectionRepository;

    public OverseeCommandHandler(
        IMediator mediator,
        ICollectionRepository collectionRepository)
    {
        _collectionRepository = collectionRepository;
        _mediator = mediator;
    }

    public async Task<OverseeCollectionResult> Handle(OverseeCommand request, CancellationToken ct)
    {
        var anyFileMoved = false;

        var collectionIds = await _collectionRepository.GetAllIds();

        foreach (var id in collectionIds)
        {
            var result = await _mediator.Send(new OverseeCollectionCommand(id), ct);
            anyFileMoved |= result.AnyFileMoved;
        }

        return new(anyFileMoved);
    }
}
