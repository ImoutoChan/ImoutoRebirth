using ImoutoRebirth.Arachne.Core.Models;
using ParserPost = Imouto.BooruParser.Post;
using SearchResult = ImoutoRebirth.Arachne.Core.Models.SearchResult;

namespace ImoutoRebirth.Arachne.Infrastructure.Abstract;

internal interface IBooruPostConverter
{
    SearchResult Convert(ParserPost post, Image image, SearchEngineType searchEngineType);
}
