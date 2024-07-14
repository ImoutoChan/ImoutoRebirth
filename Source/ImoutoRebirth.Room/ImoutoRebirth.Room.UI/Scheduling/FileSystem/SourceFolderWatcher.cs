using System.Reactive.Linq;
using System.Reactive.Subjects;
using ImoutoRebirth.Room.Application.Cqrs;
using MediatR;

namespace ImoutoRebirth.Room.UI.Scheduling.FileSystem;

/// <summary>
/// Should be registered as singleton.
/// </summary>
internal class SourceFolderWatcher
{
    private readonly IMediator _mediator;
    private readonly Subject<FileSystemWatcherEvent> _subject = new();

    private FileSystemWatcherEventStream? _currentEventStream;
    private IAsyncDisposable? _subscriptionToken;

    public SourceFolderWatcher(IMediator mediator)
    {
        Observable = _subject.ToAsyncObservable();
        _mediator = mediator;
    }
    
    public async Task Refresh(CancellationToken ct)
    {
        if (_subscriptionToken != null) 
            await _subscriptionToken.DisposeAsync();

        _currentEventStream?.Complete();

        var folders = await _mediator.Send(new OversawSourceFoldersQuery(), ct);

        _currentEventStream = new FileSystemWatcherEventStream(
            folders,
            NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
            true,
            ct);

        _subscriptionToken = await _currentEventStream.Observable
            .SubscribeAsync(
                x => _subject.OnNext(x), 
                x => _subject.OnError(x),
                () => _subject.OnCompleted());
    }

    public IAsyncObservable<FileSystemWatcherEvent> Observable { get; }
}
