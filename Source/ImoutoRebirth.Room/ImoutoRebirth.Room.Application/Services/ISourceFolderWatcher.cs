namespace ImoutoRebirth.Room.UI.Scheduling.FileSystem;

internal interface ISourceFolderWatcher
{
    Task Refresh(CancellationToken ct);
}
