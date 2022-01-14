namespace ImoutoRebirth.Navigator.Services.ImoutoViewer;

internal class ImoutoViewerArgs
{
    public Guid CollectionId { get; }

    public IReadOnlyCollection<SearchTagDto> SearchTags { get; }

    public ImoutoViewerArgs(Guid collectionId, IReadOnlyCollection<SearchTagDto> searchTags)
    {
        CollectionId = collectionId;
        SearchTags = searchTags;
    }
}