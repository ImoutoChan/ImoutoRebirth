namespace ImoutoRebirth.Room.Application.Services;

public interface ISourceFolderWatcher
{
    Task Refresh(IReadOnlyCollection<string>? folders, CancellationToken ct);
}
