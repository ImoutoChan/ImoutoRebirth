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

    public async ValueTask Execute(IJobExecutionContext context, CancellationToken ct)
    {
        if (!await Lock.WaitAsync(0, ct))
            return;

        try
        {
            var somethingNewWasSaved = await _mediator.Send(new FavoritesSaveCommand(), ct);

            if (!somethingNewWasSaved)
                return;

            await context.Scheduler.ScheduleJob<AdditionalFavoritesSaveJob>(
                TriggerBuilder.Create()
                    .WithIdentity(nameof(AdditionalFavoritesSaveJob))
                    .StartAt(DateTimeOffset.UtcNow.AddSeconds(5))
                    .WithSimpleSchedule(TimeSpan.FromSeconds(5), 12)
                    .Build(),
                cancellationToken: ct);
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
                .WithSimpleSchedule(TimeSpan.FromMinutes(_repeatEveryMinutes))
                .Build();
    }
}
