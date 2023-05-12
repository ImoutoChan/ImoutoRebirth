using ImoutoRebirth.Common.Quartz;
using ImoutoRebirth.Harpy.Services.SaveFavorites.Commands;
using MediatR;
using Microsoft.Extensions.Options;
using Quartz;

namespace ImoutoRebirth.Harpy.Services.SaveFavorites.Quartz;

[DisallowConcurrentExecution]
internal class FavoritesSaveJob : IJob
{
    private readonly IMediator _mediator;

    public FavoritesSaveJob(IMediator mediator)
    {
        _mediator = mediator;
    }

    public Task Execute(IJobExecutionContext context) => _mediator.Send(new FavoritesSaveCommand());

    public class Description : IQuartzJobDescription
    {
        private readonly int _repeatEveryMinutes;

        public Description(IOptions<FavoritesSaveJobSettings> options)
        {
            _repeatEveryMinutes = options.Value.RepeatEveryMinutes;
        }

        public IJobDetail GetJobDetails()
            => JobBuilder.Create<FavoritesSaveJob>()
                .WithIdentity("Save favorites")
                .Build();

        public ITrigger GetJobTrigger()
            => TriggerBuilder.Create()
                .WithIdentity("Save favorites trigger")
                .WithSchedule(SimpleScheduleBuilder.RepeatMinutelyForever(_repeatEveryMinutes))
                .Build();
    }
}
