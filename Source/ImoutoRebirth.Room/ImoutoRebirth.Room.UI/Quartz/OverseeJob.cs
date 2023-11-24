using ImoutoRebirth.Common.Quartz;
using ImoutoRebirth.Room.Application.Cqrs;
using MediatR;
using Microsoft.Extensions.Logging;
using Quartz;

namespace ImoutoRebirth.Room.UI.Quartz;

public class OverseeJob : IJob
{
    private static readonly SemaphoreSlim SemaphoreSlim = new(1);
    private readonly IMediator _mediator;
    private readonly ILogger<OverseeJob> _logger;

    public OverseeJob(IMediator mediator, ILogger<OverseeJob> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var ct = context.CancellationToken;
        
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

        SemaphoreSlim.Release();
    }

    public class Description : IQuartzJobDescription
    {
        public IJobDetail GetJobDetails()
            => JobBuilder.Create<OverseeJob>()
                .WithIdentity("Oversee job")
                .Build();

        public ITrigger GetJobTrigger()
            => TriggerBuilder.Create()
                .WithIdentity("Oversee trigger")
                .WithSchedule(SimpleScheduleBuilder.RepeatSecondlyForever(5))
                .Build();
    }
}
