using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ImoutoRebirth.Room.UI.Scheduling.FileSystem;

public class FileSystemWatcherEventStream
{
    private readonly ISubject<FileSystemWatcherEvent> _subject;
    private readonly List<FileSystemWatcher> _watchers = new();

    public FileSystemWatcherEventStream(
        IReadOnlyCollection<string> folders, 
        NotifyFilters filters, 
        bool includeSubdirectories,
        CancellationToken completionToken)
    {
        _subject = new Subject<FileSystemWatcherEvent>();
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
            var watcher = new FileSystemWatcher(folder)
            {
                NotifyFilter = filters,
                IncludeSubdirectories = includeSubdirectories
            };

            watcher.Changed += (_, args)
                => _subject.OnNext(new(folder, FileSystemWatcherEventType.Changed, args));
            watcher.Created += (_, args)
                => _subject.OnNext(new(folder, FileSystemWatcherEventType.Created, args));
            watcher.Deleted += (_, args)
                => _subject.OnNext(new(folder, FileSystemWatcherEventType.Deleted, args));
            watcher.Renamed += (_, args)
                => _subject.OnNext(new(folder, FileSystemWatcherEventType.Renamed, args));

            watcher.EnableRaisingEvents = true;
            
            _watchers.Add(watcher);
        }
    }

    public IAsyncObservable<FileSystemWatcherEvent> Observable { get; }
}
