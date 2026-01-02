using ImoutoRebirth.Common.Quartz;
using ImoutoRebirth.Lilin.Application.TagSlice;
using MediatR;
using Microsoft.Extensions.Options;
using Quartz;

namespace ImoutoRebirth.Lilin.UI.Quartz;

public class DeleteEmptyLocationTagsJob : IJob
{
    private readonly IMediator _mediator;

    public DeleteEmptyLocationTagsJob(IMediator mediator) => _mediator = mediator;

    public Task Execute(IJobExecutionContext context) => _mediator.Send(new DeleteEmptyLocationTagsCommand());

    public class Description : IQuartzJobDescription
    {
        private readonly int _repeatEveryMinutes;

        public Description(IOptions<DeleteEmptyLocationTagsSettings> options)
        {
            _repeatEveryMinutes = options.Value.RepeatEveryMinutes;
        }

        public IJobDetail GetJobDetails()
            => JobBuilder.Create<DeleteEmptyLocationTagsJob>()
                .WithIdentity("Delete empty location tags")
                .Build();

        public ITrigger GetJobTrigger()
            => TriggerBuilder.Create()
                .WithIdentity("Delete empty location tags trigger")
                .WithSchedule(SimpleScheduleBuilder.RepeatMinutelyForever(_repeatEveryMinutes))
                .Build();
    }
}

public class DeleteEmptyLocationTagsSettings
{
    public int RepeatEveryMinutes { get; set; } = 15;
}
