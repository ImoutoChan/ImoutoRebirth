using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Quartz;

namespace ImoutoRebirth.Common.Quartz;

internal class QuartzHostedService : IHostedService
{
    private const int WaitBeforeForcedShutdown = 30000;

    private readonly IScheduler _scheduler;
    private readonly IReadOnlyCollection<IQuartzJobDescription> _jobDescriptions;

    public QuartzHostedService(IScheduler scheduler, IEnumerable<IQuartzJobDescription> jobDescriptions)
    {
        _scheduler = scheduler;
        _jobDescriptions = jobDescriptions.ToArray();
    }

    public void Start() => StartAsync().Wait();

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await _scheduler.Start(cancellationToken);

        foreach (var jobDescription in _jobDescriptions)
            await _scheduler.ScheduleJob(
                jobDescription.GetJobDetails(), 
                jobDescription.GetJobTrigger(), 
                cancellationToken);
    }

    public void Stop() => StopAsync().Wait();

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        var schedulerShutdownTask = _scheduler.Shutdown(waitForJobsToComplete: true, cancellationToken);
        var timeoutTask = Task.Delay(WaitBeforeForcedShutdown, cancellationToken);

        await Task.WhenAny(schedulerShutdownTask, timeoutTask);

        if (!_scheduler.IsShutdown)
            await _scheduler.Shutdown(cancellationToken);
    }
}