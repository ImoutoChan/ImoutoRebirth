using ImoutoRebirth.Navigator.Services.Tags.Model;
using ImoutoRebirth.Navigator.ViewModel.ListEntries;

namespace ImoutoRebirth.Navigator.Services;

internal interface IFileLoadingService
{
    Task LoadFiles(
        int bulkFactor,
        int previewSize,
        Guid? collectionId,
        IReadOnlyCollection<SearchTag> searchTags,
        Action<int> counterUpdater,
        Action<IReadOnlyCollection<INavigatorListEntry>, CancellationToken> entryUpdater,
        Action rollbackAction,
        Action initAction,
        Action finishAction,
        int skip = 0);

    Task<int> GetCount(
        Guid? collectionId,
        IReadOnlyCollection<SearchTag> searchTags,
        CancellationToken token = default);
}
