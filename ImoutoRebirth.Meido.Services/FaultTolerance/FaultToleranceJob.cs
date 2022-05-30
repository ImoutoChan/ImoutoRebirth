using ImoutoRebirth.Common.Quartz;
using ImoutoRebirth.Meido.Services.FaultTolerance.CqrsCommands;
using MediatR;
using Microsoft.Extensions.Options;
using Quartz;

namespace ImoutoRebirth.Meido.Services.FaultTolerance;

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

    public async Task Execute(IJobExecutionContext context)
    {
        if (!_faultToleranceSettings.Value.IsEnabled)
            return;

        await _mediator.Send(new RequeueFaultsCommand());
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
                .WithSchedule(SimpleScheduleBuilder.RepeatMinutelyForever(_repeatEveryMinutes))
                .Build();
    }
}