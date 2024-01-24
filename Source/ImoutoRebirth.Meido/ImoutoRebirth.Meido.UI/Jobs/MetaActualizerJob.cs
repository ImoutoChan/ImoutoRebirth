using ImoutoRebirth.Common.Quartz;
using ImoutoRebirth.Meido.Application.SourceActualizingStateSlice.Commands;
using MediatR;
using Microsoft.Extensions.Options;
using Quartz;

namespace ImoutoRebirth.Meido.UI.Jobs;

[DisallowConcurrentExecution]
internal class MetaActualizerJob : IJob
{
    private readonly IMediator _mediator;
    private readonly IOptionsSnapshot<MetadataActualizerSettings> _actualizerSettings;

    public MetaActualizerJob(IMediator mediator, IOptionsSnapshot<MetadataActualizerSettings> actualizerSettings)
    {
        _mediator = mediator;
        _actualizerSettings = actualizerSettings;
    }

    public async Task Execute(IJobExecutionContext context) 
        => await _mediator.Send(new RequestActualizationCommand(_actualizerSettings.Value.ActiveSources ?? []));

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
