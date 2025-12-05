using ImoutoRebirth.Room.Application.Cqrs.IntegritySlice;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ImoutoRebirth.Room.UI;

internal class IntegrityReportJobBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public IntegrityReportJobBackgroundService(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var runner = scope.ServiceProvider.GetRequiredService<IIntegrityReportJobRunner>();

            await runner.BuildNextUnfinishedReport(stoppingToken);

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}

