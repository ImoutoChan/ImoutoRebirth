using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ImoutoViewer.ImoutoRebirth.Services.Tags;
using ImoutoViewer.ImoutoRebirth.Services.Tags.Model;

namespace ImoutoViewer.ImoutoRebirth.Services
{
    internal class FileLoadingService : IFileLoadingService
    {
        private readonly IFileService _fileService;

        public FileLoadingService(IFileService fileService)
        {
            _fileService = fileService;
        }

        public async Task<IList<string>> LoadFiles(
            Guid? collectionId,
            IReadOnlyCollection<SearchTag> searchTags,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await LoadEntries(collectionId, searchTags, cancellationToken);
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
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

        private async Task<IList<string>> LoadEntries(
            Guid? collectionId,
            IReadOnlyCollection<SearchTag> searchTags,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var found = await _fileService.SearchFiles(
                collectionId,
                searchTags,
                cancellationToken);

            return found.Select(x => x.Path).Select(x => "Q" + x.Substring(1)).ToList();
        }

        private async Task<int> GetCount(
            Guid? collectionId,
            IReadOnlyCollection<SearchTag> searchTags,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await _fileService.CountFiles(
                collectionId,
                searchTags,
                cancellationToken);
        }
    }
}