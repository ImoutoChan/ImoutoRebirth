using ImoutoRebirth.Common.Quartz;
using ImoutoRebirth.Room.Application.Cqrs;
using MediatR;
using Quartz;

namespace ImoutoRebirth.Room.UI.Quartz;

public class OverseeJob : IJob
{
    private readonly IMediator _mediator;

    public OverseeJob(IMediator mediator) => _mediator = mediator;

    public async Task Execute(IJobExecutionContext context) => await _mediator.Send(new OverseeCommand(true));

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
