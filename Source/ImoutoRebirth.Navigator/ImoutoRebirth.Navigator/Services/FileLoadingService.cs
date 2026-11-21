using System.Windows;
using ImoutoRebirth.Common;
using ImoutoRebirth.Navigator.Services.Tags;
using ImoutoRebirth.Navigator.Services.Tags.Model;
using ImoutoRebirth.Navigator.ViewModel.ListEntries;

namespace ImoutoRebirth.Navigator.Services;

internal class FileLoadingService : IFileLoadingService
{
    private CancellationTokenSource _lastOperationCts = new();
    private readonly SemaphoreSlim _operationLocker = new(1);
    private readonly object _ctsLocker = new();
    private readonly IFileService _fileService;

    public FileLoadingService(IFileService fileService)
    {
        _fileService = fileService;
    }

    public async Task LoadFiles(
        int bulkFactor,
        int previewSize,
        Guid? collectionId,
        IReadOnlyCollection<SearchTag> searchTags,
        Action<int> counterUpdater,
        Action<IReadOnlyCollection<INavigatorListEntry>, CancellationToken> entryUpdater,
        Action rollbackAction,
        Action initAction,
        Action finishAction,
        int skip = 0)
    {
        var newCts = new CancellationTokenSource();
        var ct = newCts.Token;

        lock (_ctsLocker)
        {
            _lastOperationCts.Cancel();
            _lastOperationCts = newCts;
        }

        await _operationLocker.WaitAsync(ct);

        try
        {
            initAction();

            var counterTask = UpdateCounter(collectionId, searchTags, counterUpdater, ct);
            var loadTask = BulkLoadEntries(bulkFactor, previewSize, collectionId, searchTags, entryUpdater, skip, ct);

            await Task.WhenAll(counterTask, loadTask);

            finishAction();
        }
        catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException)
        {
            rollbackAction();
        }
        finally
        {
            _operationLocker.Release();
        }
    }

    private async Task<int> UpdateCounter(
        Guid? collectionId,
        IReadOnlyCollection<SearchTag> searchTags,
        Action<int> counterUpdater,
        CancellationToken ct)
    {
        var count = await GetCount(collectionId, searchTags, ct);
        counterUpdater(count);
        return count;
    }

    private async Task BulkLoadEntries(
        int bulkFactor,
        int previewSize,
        Guid? collectionId,
        IReadOnlyCollection<SearchTag> searchTags,
        Action<IReadOnlyCollection<INavigatorListEntry>, CancellationToken> entryUpdater,
        int skip,
        CancellationToken ct)
    {
        var page = 0;
        bool cont;

        do
        {
            ct.ThrowIfCancellationRequested();

            var entries = await LoadNewEntries(
                count: bulkFactor,
                skip: page * bulkFactor,
                previewSize: previewSize,
                collectionId: collectionId,
                searchTags: searchTags,
                ct: ct);

            page++;
            cont = entries.Continue;

            entryUpdater(entries.Item1, ct);
        } while (cont);
    }

    private async Task<(IReadOnlyCollection<INavigatorListEntry> Files, bool Continue)> LoadNewEntries(
        int count,
        int skip,
        int previewSize,
        Guid? collectionId,
        IReadOnlyCollection<SearchTag> searchTags,
        CancellationToken ct)
    {
        var entries = await Task.Run(NavigatorListEntries, ct);

        return entries;

        async Task<(IReadOnlyCollection<INavigatorListEntry> Files, bool Continue)> NavigatorListEntries()
        {
            var result = await _fileService.SearchFiles(
                collectionId,
                searchTags,
                count,
                skip,
                ct);

            var navigatorListEntries = result.Files
                .Select(
                    x => EntryVMFactory.CreateListEntry(
                        x.Path,
                        new Size(previewSize, previewSize),
                        x.Id))
                .WhereNotNull()
                .SkipExceptions()
                .WithCancellation(ct)
                .ToList();

            return (navigatorListEntries, result.Continue);
        }
    }

    public async Task<int> GetCount(
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
