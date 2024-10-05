using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Room.UI.FileSystemEvents;

public enum FileSystemWatcherEventType { Changed, Created, Deleted, Renamed }

public record FileSystemWatcherEvent(string Folder, FileSystemWatcherEventType Type, FileSystemEventArgs Args);

/// <summary>
/// Encapsulation of FileSystemWatcher, that provide reactive event stream for the provided folders. 
/// </summary>
public class FileSystemWatcherEventStream
{
    private readonly ISubject<FileSystemWatcherEvent> _subject = new Subject<FileSystemWatcherEvent>();
    private readonly List<FileSystemWatcher> _watchers = new();

    public FileSystemWatcherEventStream(
        IReadOnlyCollection<string> folders, 
        NotifyFilters filters, 
        bool includeSubdirectories,
        CancellationToken completionToken, 
        ILogger logger)
    {
        Observable = _subject.ToAsyncObservable();

        completionToken.Register(() =>
        {
            foreach (var watcher in _watchers)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }

            _subject.OnCompleted();
        });
        
        foreach (var folder in folders)
        {
            if (!Directory.Exists(folder))
                continue;
            
            var watcher = new FileSystemWatcher(folder)
            {
                NotifyFilter = filters,
                IncludeSubdirectories = includeSubdirectories
            };

            watcher.Changed += (_, args)
                => _subject.OnSafeNext(new(folder, FileSystemWatcherEventType.Changed, args), logger);
            watcher.Created += (_, args)
                => _subject.OnSafeNext(new(folder, FileSystemWatcherEventType.Created, args), logger);
            watcher.Deleted += (_, args)
                => _subject.OnSafeNext(new(folder, FileSystemWatcherEventType.Deleted, args), logger);
            watcher.Renamed += (_, args)
                => _subject.OnSafeNext(new(folder, FileSystemWatcherEventType.Renamed, args), logger);

            watcher.EnableRaisingEvents = true;
            
            _watchers.Add(watcher);
        }
    }
    
    public void Complete()
    {
        foreach (var watcher in _watchers)
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
        }

        _subject.OnCompleted();
    }

    public IAsyncObservable<FileSystemWatcherEvent> Observable { get; }
}

public static class ObserverExtensions
{
    public static void OnSafeNext<T>(this IObserver<T> observer, T value, ILogger? logger = null)
    {
        try
        {
            observer.OnNext(value);
        }
        catch (Exception e)
        {
            logger?.LogWarning(e, "Error while trying to observe next value");
        }
    }
}
