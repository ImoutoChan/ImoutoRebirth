using System.Diagnostics;
using System.Reactive.Linq;
using ImoutoRebirth.Room.Application.Cqrs;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ImoutoRebirth.Room.UI.FileSystemEvents;

internal class SourceFolderWatcherHostedService : BackgroundService
{
    private static readonly ActivitySource ActivitySource = new("ImoutoRebirth");

    private readonly IServiceProvider _serviceProvider;
    private readonly SourceFolderWatcher _watcher;

    public SourceFolderWatcherHostedService(IServiceProvider serviceProvider, SourceFolderWatcher watcher)
    {
        _serviceProvider = serviceProvider;
        _watcher = watcher;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _watcher.Refresh(null, stoppingToken);
        await RunChangesReactor();

        while (!stoppingToken.IsCancellationRequested) 
            await Task.Delay(1000, stoppingToken);
    }
    
    private async Task RunChangesReactor()
    {
        await _watcher.Observable
            .Where(x => x.Type != FileSystemWatcherEventType.Deleted)
            .Throttle(TimeSpan.FromMilliseconds(250))
            .SubscribeAsync(async _ => await TriggerOversee());
    }

    private async Task TriggerOversee()
    {
        Activity.Current = null;
        using var activity = ActivitySource.CreateActivity("Oversee", ActivityKind.Internal)?.Start();
        using var scope = _serviceProvider.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.Send(new OverseeCommand());
    }
}
