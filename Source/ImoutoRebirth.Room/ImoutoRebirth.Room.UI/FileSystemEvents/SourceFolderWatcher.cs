using System.Reactive.Linq;
using System.Reactive.Subjects;
using ImoutoRebirth.Room.Application.Cqrs;
using ImoutoRebirth.Room.Application.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Room.UI.FileSystemEvents;

/// <summary>
/// Dynamic storage for current source folders. Can also be refreshed. Should be registered as singleton.
/// </summary>
internal class SourceFolderWatcher : ISourceFolderWatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISubject<System.Reactive.Unit> _changes = Subject.Synchronize(new Subject<System.Reactive.Unit>());
    private readonly ILogger<SourceFolderWatcher> _logger;

    private FileSystemWatcherEventStream? _currentEventStream;
    private IAsyncDisposable? _subscriptionToken;

    public SourceFolderWatcher(IServiceProvider serviceProvider, ILogger<SourceFolderWatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        Changes = _changes.ToAsyncObservable();
    }
    
    public async Task Refresh(IReadOnlyCollection<string>? folders, CancellationToken ct)
    {
        _logger.LogInformation("Refreshing source folders");

        if (_subscriptionToken != null) 
            await _subscriptionToken.DisposeAsync();

        _currentEventStream?.Complete();

        folders ??= await GetSourceFolders(ct);

        _logger.LogInformation("Watching folders: {Folders}", string.Join(", ", folders));
        _currentEventStream = new FileSystemWatcherEventStream(
            folders,
            NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
            true,
            ct,
            _logger);

        _subscriptionToken = await _currentEventStream.Observable
            .Where(x => x.Type != FileSystemWatcherEventType.Deleted)
            .SubscribeAsync(_ => _changes.OnNext(System.Reactive.Unit.Default));

        _changes.OnNext(System.Reactive.Unit.Default);
    }
    
    private async Task<IReadOnlyCollection<string>> GetSourceFolders(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        return await mediator.Send(new OversawSourceFoldersQuery(), ct);
    }

    public IAsyncObservable<System.Reactive.Unit> Changes { get; }
}
