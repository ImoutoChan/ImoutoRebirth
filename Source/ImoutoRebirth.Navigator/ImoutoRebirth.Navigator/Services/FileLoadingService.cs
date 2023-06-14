using System.Windows;
using ImoutoRebirth.LilinService.WebApi.Client;
using ImoutoRebirth.Navigator.Extensions;
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
    private readonly FilesClient _filesClient;

    public FileLoadingService(IFileService fileService, FilesClient filesClient)
    {
        _fileService = fileService;
        _filesClient = filesClient;
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
        var token = newCts.Token;

        lock (_ctsLocker)
        {
            _lastOperationCts.Cancel();
            _lastOperationCts = newCts;
        }

        await _operationLocker.WaitAsync(token);

        try
        {
            initAction();

            var counterTask = UpdateCounter(collectionId, searchTags, counterUpdater, token);
            var loadTask = BulkLoadEntries(bulkFactor, previewSize, collectionId, searchTags, entryUpdater, skip, token);

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
        CancellationToken token)
    {
        var count = await GetCount(collectionId, searchTags, token);
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
        CancellationToken token)
    {
        var page = 0;
        bool cont;

        do
        {
            token.ThrowIfCancellationRequested();

            var entries = await LoadNewEntries(
                count: bulkFactor,
                skip: page * bulkFactor,
                previewSize: previewSize,
                collectionId: collectionId,
                searchTags: searchTags,
                token: token);

            page++;
            cont = entries.Continue;

            entryUpdater(entries.Item1, token);
        } while (cont);
    }

    private async Task<(IReadOnlyCollection<INavigatorListEntry> Files, bool Continue)> LoadNewEntries(
        int count,
        int skip,
        int previewSize,
        Guid? collectionId,
        IReadOnlyCollection<SearchTag> searchTags,
        CancellationToken token)
    {
        var entries = await Task.Run(async () => await NavigatorListEntries(token), token);

        return entries;

        async Task<(List<INavigatorListEntry> Files, bool Continue)> NavigatorListEntries(CancellationToken ct)
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
                        _filesClient,
                        x.Id))
                .Where(x => x != null)
                .SkipExceptions()
                .WithCancellation(ct)
                .ToList();

            return (navigatorListEntries, result.Continue);
        }
    }

    public async Task<int> GetCount(
        Guid? collectionId,
        IReadOnlyCollection<SearchTag> searchTags,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        return await _fileService.CountFiles(
            collectionId,
            searchTags,
            token);
    }
}
