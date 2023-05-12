using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Spi;

namespace ImoutoRebirth.Common.Quartz;

internal class QuartzJobFactory : IJobFactory
{
    private readonly ILogger<QuartzJobFactory> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ConcurrentDictionary<IJob, IServiceScope> _scopes;

    public QuartzJobFactory(ILogger<QuartzJobFactory> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _scopes = new ConcurrentDictionary<IJob, IServiceScope>();
    }

    public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
    {
        var scope = _serviceScopeFactory.CreateScope();
        try
        {
            _logger.LogTrace("Producing instance of Job '{JobKey}', class={JobClass}", 
                bundle.JobDetail.Key, 
                bundle.JobDetail.JobType.FullName);

            var job = (IJob)scope.ServiceProvider.GetRequiredService(bundle.JobDetail.JobType);

            _scopes.TryAdd(job, scope);

            return job;
        }
        catch (Exception exception)
        {
            scope.Dispose();
            _logger.LogError("An exception is occured in NewJob method", exception);
            throw;
        }
    }

    public void ReturnJob(IJob job)
    {
        _logger.LogTrace("Returning instance of Job");
        (job as IDisposable)?.Dispose();

        if (!_scopes.TryRemove(job, out var scope))
            return;

        scope.Dispose();
    }
}