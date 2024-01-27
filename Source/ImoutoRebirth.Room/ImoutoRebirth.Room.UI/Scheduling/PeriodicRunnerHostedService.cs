using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace ImoutoRebirth.Room.UI.Scheduling;

internal class PeriodicRunnerHostedService : BackgroundService
{
    private readonly IOptions<PeriodicRunnerOptions> _options;
    private readonly IServiceProvider _serviceProvider;

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

    private async Task RunJob(Type type, CancellationToken stoppingToken)
    {
        do
        {
            using var scope = _serviceProvider.CreateScope();

            if (scope.ServiceProvider.GetRequiredService(type) is IPeriodicRunningJob job)
            {
                await job.Run(stoppingToken);
                await Task.Delay(job.PeriodDelay, stoppingToken);
            }
            else
            {
                break;
            }
        } while (!stoppingToken.IsCancellationRequested);
    }
}
