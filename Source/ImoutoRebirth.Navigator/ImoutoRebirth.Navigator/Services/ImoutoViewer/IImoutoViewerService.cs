using ImoutoRebirth.Navigator.Services.Tags.Model;

namespace ImoutoRebirth.Navigator.Services.ImoutoViewer;

internal interface IImoutoViewerService
{
    Task OpenFile(string path, Guid? collectionId, IEnumerable<SearchTag> searchTags, IEnumerable<string> playlist);
}
