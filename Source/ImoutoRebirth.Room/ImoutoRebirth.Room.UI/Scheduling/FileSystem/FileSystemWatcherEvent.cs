namespace ImoutoRebirth.Room.UI.Scheduling.FileSystem;

public record FileSystemWatcherEvent(string Folder, FileSystemWatcherEventType Type, FileSystemEventArgs Args);
