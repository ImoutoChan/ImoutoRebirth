using Microsoft.Extensions.Hosting;
using Quartz;

namespace ImoutoRebirth.Common.Quartz;

internal class QuartzJobsScheduler : BackgroundService
{
    private readonly IScheduler _scheduler;
    private readonly IReadOnlyCollection<IQuartzJobDescription> _jobDescriptions;

    public QuartzJobsScheduler(IScheduler scheduler, IEnumerable<IQuartzJobDescription> jobDescriptions)
    {
        _scheduler = scheduler;
        _jobDescriptions = jobDescriptions.ToArray();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        foreach (var jobDescription in _jobDescriptions)
        {
            await _scheduler.ScheduleJob(
                jobDescription.GetJobDetails(),
                jobDescription.GetJobTrigger(),
                cancellationToken: stoppingToken);
        }
    }
}
