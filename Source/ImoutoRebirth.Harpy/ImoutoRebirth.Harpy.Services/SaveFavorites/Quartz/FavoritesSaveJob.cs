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
    public static readonly SemaphoreSlim Lock = new(1, 1);

    public FavoritesSaveJob(IMediator mediator) => _mediator = mediator;

    public async Task Execute(IJobExecutionContext context)
    {
        if (!await Lock.WaitAsync(0))
            return;

        try
        {
            var somethingNewWasSaved = await _mediator.Send(new FavoritesSaveCommand());

            if (!somethingNewWasSaved)
                return;

            await context.Scheduler.ScheduleJob(
                JobBuilder.Create<AdditionalFavoritesSaveJob>()
                    .WithIdentity(nameof(AdditionalFavoritesSaveJob))
                    .Build(),
                TriggerBuilder.Create()
                    .WithIdentity(nameof(AdditionalFavoritesSaveJob))
                    .StartAt(DateBuilder.FutureDate(5, IntervalUnit.Second))
                    .WithSimpleSchedule(x => x.WithIntervalInSeconds(5).WithRepeatCount(12))
                    .Build());
        }
        finally
        {
            Lock.Release();
        }
    }

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
