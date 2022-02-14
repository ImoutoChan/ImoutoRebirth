using ImoutoRebirth.Common;

namespace ImoutoRebirth.Arachne.Core.Models;

public abstract class SearchResult
{
    public Image Image { get; }

    public SearchEngineType Source { get; }

    protected SearchResult(Image image, SearchEngineType source)
    {
        ArgumentValidator.NotNull(() => image);

        Image = image;
        Source = source;
    }
}