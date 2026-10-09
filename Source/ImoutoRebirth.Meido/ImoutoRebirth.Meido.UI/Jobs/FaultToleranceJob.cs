using ImoutoRebirth.Common.Quartz;
using ImoutoRebirth.Meido.Application.ParsingStatusSlice.Commands;
using MediatR;
using Microsoft.Extensions.Options;
using Quartz;

namespace ImoutoRebirth.Meido.UI.Jobs;

[DisallowConcurrentExecution]
public class FaultToleranceJob : IJob
{
    private readonly IMediator _mediator;
    private readonly IOptionsSnapshot<FaultToleranceSettings> _faultToleranceSettings;

    public FaultToleranceJob(
        IMediator mediator, 
        IOptionsSnapshot<FaultToleranceSettings> faultToleranceSettings)
    {
        _mediator = mediator;
        _faultToleranceSettings = faultToleranceSettings;
    }

    public async ValueTask Execute(IJobExecutionContext context, CancellationToken ct)
    {
        if (!_faultToleranceSettings.Value.IsEnabled)
            return;

        await _mediator.Send(new RequeueFaultedParsingsCommand(), ct);
    }

    public class Description : IQuartzJobDescription
    {
        private readonly int _repeatEveryMinutes;

        public Description(IOptions<FaultToleranceSettings> options)
        {
            _repeatEveryMinutes = options.Value.RepeatEveryMinutes;
        }

        public IJobDetail GetJobDetails()
            => JobBuilder.Create<FaultToleranceJob>()
                .WithIdentity("FaultTolerance job")
                .Build();

        public ITrigger GetJobTrigger()
            => TriggerBuilder.Create()
                .WithIdentity("FaultTolerance trigger")
                .WithSimpleSchedule(TimeSpan.FromMinutes(_repeatEveryMinutes))
                .Build();
    }
}
