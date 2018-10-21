using ImoutoRebirth.Common;

namespace ImoutoRebirth.Shop.Core.Models
{
    public class SearchError : SearchResult
    {
        public string Error { get; }

        public SearchError(Image image, SearchEngineType source, string error) 
            : base(image, source)
        {
            ArgumentValidator.NotNull(() => image, () => error);

            Error = error;
        }
    }
}