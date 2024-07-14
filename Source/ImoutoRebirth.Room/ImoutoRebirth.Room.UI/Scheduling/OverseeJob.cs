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

    public TimeSpan PeriodDelay { get; private set; } = TimeSpan.FromSeconds(5);
    
    public bool RequestRapidRun { get; private set; }

    public async Task Run(CancellationToken ct)
    {
        try
        {
            var result = await _mediator.Send(new OverseeCommand(), ct);

            if (result.AnyFileMoved)
            {
                PeriodDelay = TimeSpan.FromMilliseconds(500);
                RequestRapidRun = true;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Oversee process error");
        }
    }
}
