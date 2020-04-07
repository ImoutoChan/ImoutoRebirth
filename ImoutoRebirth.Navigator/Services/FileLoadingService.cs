using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Imouto.Utils.Core;
using ImoutoRebirth.Navigator.Services.Tags;
using ImoutoRebirth.Navigator.Services.Tags.Model;
using ImoutoRebirth.Navigator.ViewModel;

namespace ImoutoRebirth.Navigator.Services
{
    internal class FileLoadingService : IFileLoadingService
    {
        private CancellationTokenSource _lastOperationCts = new CancellationTokenSource();
        private readonly SemaphoreSlim _operationLocker = new SemaphoreSlim(1);
        private readonly object _ctsLocker = new object();
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
            Action finishAction)
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

                var count = await GetCount(collectionId, searchTags, token);
                counterUpdater(count);
                
                if (count == 0)
                {
                    finishAction();
                    return;
                }

                await BulkLoadEntries(bulkFactor, count, previewSize, collectionId, searchTags, entryUpdater, token);

                finishAction();
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                rollbackAction();
            }
            finally
            {
                _operationLocker.Release();
            }
        }

        private async Task BulkLoadEntries(
            int bulkFactor,
            int totalCount,
            int previewSize,
            Guid? collectionId,
            IReadOnlyCollection<SearchTag> searchTags,
            Action<IReadOnlyCollection<INavigatorListEntry>, CancellationToken> entryUpdater,
            CancellationToken token)
        {
            for (var i = totalCount; i > 0; i -= bulkFactor)
            {
                token.ThrowIfCancellationRequested();

                var entries = await LoadNewEntries(
                    count: bulkFactor, 
                    skip: totalCount - i, 
                    previewSize: previewSize, 
                    collectionId: collectionId, 
                    searchTags: searchTags, 
                    token: token);

                entryUpdater(entries, token);
            }
        }

        private async Task<IReadOnlyCollection<INavigatorListEntry>> LoadNewEntries(
            int count,
            int skip,
            int previewSize,
            Guid? collectionId,
            IReadOnlyCollection<SearchTag> searchTags,
            CancellationToken token)
        {
            var entries = await Task.Run(async () => await NavigatorListEntries(token), token);

            return entries;

            async Task<List<INavigatorListEntry>> NavigatorListEntries(CancellationToken ct)
            {
                var found = await _fileService.SearchFiles(
                    collectionId,
                    searchTags,
                    count,
                    skip,
                    ct);

                var navigatorListEntries = found
                    .Select(x => EntryVM.GetListEntry(x.Path, new Size(previewSize, previewSize), x.Id))
                    .SkipExceptions()
                    .WithCancellation(ct)
                    .ToList();

                return navigatorListEntries;
            }
        }

        private async Task<int> GetCount(
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
}