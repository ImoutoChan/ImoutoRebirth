using System.Diagnostics;
using ImoutoViewer.ImoutoRebirth.Services.Tags;
using ImoutoViewer.ImoutoRebirth.Services.Tags.Model;

namespace ImoutoViewer.ImoutoRebirth.Services;

internal class FileLoadingService : IFileLoadingService
{
    private readonly IFileService _fileService;

    public FileLoadingService(IFileService fileService)
    {
        _fileService = fileService;
    }

    public async Task<IReadOnlyCollection<string>> LoadFiles(
        Guid? collectionId,
        IReadOnlyCollection<SearchTag> searchTags,
        CancellationToken ct = default)
    {
        try
        {
            return await LoadEntries(collectionId, searchTags, ct);
        }
        catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException)
        {
            Debug.WriteLine(ex.ToString());
            return ArraySegment<string>.Empty;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
            return ArraySegment<string>.Empty;
        }
    }

    private async Task<IReadOnlyCollection<string>> LoadEntries(
        Guid? collectionId,
        IReadOnlyCollection<SearchTag> searchTags,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var found = await _fileService.SearchFiles(
            collectionId,
            searchTags,
            ct);

        return found.Select(x => x.Path).ToList();
    }

    private async Task<int> GetCount(
        Guid? collectionId,
        IReadOnlyCollection<SearchTag> searchTags,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        return await _fileService.CountFiles(
            collectionId,
            searchTags,
            ct);
    }
}
