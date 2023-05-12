using ImoutoRebirth.Common.Quartz;
using ImoutoRebirth.Meido.Services.MetadataActualizer.CqrsCommands;
using MediatR;
using Microsoft.Extensions.Options;
using Quartz;

namespace ImoutoRebirth.Meido.Services.MetadataActualizer;

[DisallowConcurrentExecution]
public class MetaActualizerJob : IJob
{
    private readonly IMediator _mediator;

    public MetaActualizerJob(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Execute(IJobExecutionContext context) 
        => await _mediator.Send(new ActualizeSourcesCommand());

    public class Description : IQuartzJobDescription
    {
        private readonly int _repeatEveryMinutes;

        public Description(IOptions<MetadataActualizerSettings> options)
        {
            _repeatEveryMinutes = options.Value.RepeatEveryMinutes;
        }

        public IJobDetail GetJobDetails()
            => JobBuilder.Create<MetaActualizerJob>()
                .WithIdentity("MetaActualizer job")
                .Build();

        public ITrigger GetJobTrigger()
            => TriggerBuilder.Create()
                .WithIdentity("Oversee trigger")
                .WithSchedule(SimpleScheduleBuilder.RepeatMinutelyForever(_repeatEveryMinutes))
                .Build();
    }
}