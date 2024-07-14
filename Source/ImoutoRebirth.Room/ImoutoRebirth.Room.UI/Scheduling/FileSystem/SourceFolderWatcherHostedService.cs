using System.Diagnostics;
using System.Reactive.Linq;
using ImoutoRebirth.Room.Application.Cqrs;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ImoutoRebirth.Room.UI.Scheduling.FileSystem;

internal class SourceFolderWatcherHostedService : BackgroundService
{
    private static readonly ActivitySource ActivitySource = new("ImoutoRebirth");

    private readonly IServiceProvider _serviceProvider;
    
    private FileSystemWatcherEventStream? _eventStream;

    public SourceFolderWatcherHostedService(IServiceProvider serviceProvider) 
        => _serviceProvider = serviceProvider;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await SubscribeToSourceFolders(stoppingToken);
        await RunChangesReactor();

        while (!stoppingToken.IsCancellationRequested) 
            await Task.Delay(1000, stoppingToken);

        // somehow get updates about new source folders
    }

    private async Task SubscribeToSourceFolders(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var folders = await mediator.Send(new OversawSourceFoldersQuery(), ct);

        _eventStream = new FileSystemWatcherEventStream(
            folders,
            NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName, 
            true,
            ct);
    }
    
    private async Task RunChangesReactor()
    {
        if (_eventStream == null)
            throw new InvalidOperationException("Event stream is not initialized");
        
        await _eventStream.Observable
            .Throttle(TimeSpan.FromMilliseconds(250))
            .SubscribeAsync(async _ => await TriggerOversee());
    }

    private async Task TriggerOversee()
    {
        using var activity = ActivitySource.CreateActivity("Oversee", ActivityKind.Internal)?.Start();
        using var scope = _serviceProvider.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.Send(new OverseeCommand());
    }
}
