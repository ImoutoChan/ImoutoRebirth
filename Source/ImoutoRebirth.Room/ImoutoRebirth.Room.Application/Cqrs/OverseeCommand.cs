using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Room.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Room.Application.Cqrs;

public record OverseeCommand(bool RepeatWhenNewFilesAreDiscovered) : ICommand;

internal class OverseeCommandHandler : ICommandHandler<OverseeCommand>
{
    private static readonly SemaphoreSlim SemaphoreSlim = new(1);
    private readonly IMediator _mediator;
    private readonly ICollectionRepository _collectionRepository;
    private readonly ILogger _logger;

    public OverseeCommandHandler(
        IMediator mediator,
        ICollectionRepository collectionRepository,
        ILogger<OverseeCommandHandler> logger)
    {
        _collectionRepository = collectionRepository;
        _logger = logger;
        _mediator = mediator;
    }

    public async Task Handle(OverseeCommand request, CancellationToken ct)
    {
        if (!await SemaphoreSlim.WaitAsync(0, ct))
        {
            _logger.LogTrace("Oversee process have not finished yet");
            return;
        }

        var runMoreTimes = 0;
        do
        {
            var anyFileMoved = false;
            try
            {
                var collectionIds = await _collectionRepository.GetAllIds();

                foreach (var id in collectionIds)
                {
                    var result = await _mediator.Send(new OverseeCollectionCommand(id), ct);
                    anyFileMoved |= result.AnyFileMoved;
                }
                
                if (!request.RepeatWhenNewFilesAreDiscovered)
                    break;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Oversee process error");
                break;
            }

            if (anyFileMoved)
                runMoreTimes = 10;

            runMoreTimes--;
            if (runMoreTimes > 0)
                await Task.Delay(500, ct);

        } while (runMoreTimes > 0);
            
        SemaphoreSlim.Release();
    }
}
