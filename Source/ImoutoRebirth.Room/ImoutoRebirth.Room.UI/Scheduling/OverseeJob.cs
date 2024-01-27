using ImoutoRebirth.Room.Application.Cqrs;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Room.UI.Scheduling;

internal class OverseeJob : IPeriodicRunningJob
{
    private readonly IMediator _mediator;
    private readonly ILogger<OverseeJob> _logger;

    public OverseeJob(IMediator mediator, ILogger<OverseeJob> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public TimeSpan PeriodDelay => TimeSpan.FromSeconds(5);

    public async Task Run(CancellationToken ct)
    {
        var runMoreTimes = 0;
        do
        {
            var anyFileMoved = false;
            try
            {
                anyFileMoved |= (await _mediator.Send(new OverseeCommand(), ct)).AnyFileMoved;
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
    }
}
