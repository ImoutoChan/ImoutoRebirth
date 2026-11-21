using ImoutoViewer.ImoutoRebirth.Services.Tags.Model;

namespace ImoutoViewer.ImoutoRebirth.Services;

internal interface IFileLoadingService
{
    Task<IReadOnlyCollection<string>> LoadFiles(
        Guid? collectionId,
        IReadOnlyCollection<SearchTag> searchTags,
        CancellationToken ct = default);
}