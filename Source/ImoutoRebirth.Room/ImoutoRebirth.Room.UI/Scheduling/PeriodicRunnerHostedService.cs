using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace ImoutoRebirth.Room.UI.Scheduling;

internal class PeriodicRunnerHostedService : BackgroundService
{
    private static readonly ActivitySource ActivitySource = new("ImoutoRebirth");

    private readonly IOptions<PeriodicRunnerOptions> _options;
    private readonly IServiceProvider _serviceProvider;

    private int _rapidRunCounter = 0;
    private TimeSpan _rapidRunDelay = TimeSpan.MaxValue;

    public PeriodicRunnerHostedService(IOptions<PeriodicRunnerOptions> options, IServiceProvider serviceProvider)
    {
        _options = options;
        _serviceProvider = serviceProvider;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tasks = new List<Task>();

        foreach (var jobType in _options.Value.JobTypes)
        {
            var task = RunJob(jobType, stoppingToken);
            tasks.Add(task);
        }
        
        return Task.WhenAll(tasks);
    }

    private async Task RunJob(Type type, CancellationToken ct)
    {
        do
        {
            using var scope = _serviceProvider.CreateScope();

            if (scope.ServiceProvider.GetRequiredService(type) is IPeriodicRunningJob job)
            {
                await RunPeriodicJob(job, ct);
            }
            else
            {
                break;
            }

        } while (!ct.IsCancellationRequested);
    }

    private async Task RunPeriodicJob(IPeriodicRunningJob job, CancellationToken ct)
    {
        using var activity = ActivitySource.CreateActivity(job.GetType().Name, ActivityKind.Internal)?.Start();
        await job.Run(ct);
        activity?.Stop();
        
        if (_rapidRunCounter > 0)
        {
            _rapidRunCounter--;
            await Task.Delay(_rapidRunDelay, ct);
        }
        else
        {
            await Task.Delay(job.PeriodDelay, ct);
        }

        if (job.RequestRapidRun)
        {
            _rapidRunCounter = 10;
            _rapidRunDelay = job.PeriodDelay;
        }
    }
}
