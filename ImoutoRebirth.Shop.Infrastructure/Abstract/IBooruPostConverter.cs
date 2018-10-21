using ImoutoRebirth.Shop.Core.Models;
using ParserPost = Imouto.BooruParser.Model.Base.Post;
using SearchResult = ImoutoRebirth.Shop.Core.Models.SearchResult;

namespace ImoutoRebirth.Shop.Infrastructure.Abstract
{
    internal interface IBooruPostConverter
    {
        SearchResult Convert(ParserPost post, Image image, SearchEngineType searchEngineType);
    }
}