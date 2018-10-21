using System.Linq;
using System.Threading.Tasks;
using Imouto.BooruParser.Loaders;
using Imouto.BooruParser.Model.Base;
using ImoutoRebirth.Shop.Core.InfrastructureContracts;
using ImoutoRebirth.Shop.Core.Models;
using ImoutoRebirth.Shop.Infrastructure.Abstract;
using Mackiovello.Maybe;
using SearchResult = ImoutoRebirth.Shop.Core.Models.SearchResult;

namespace ImoutoRebirth.Shop.Infrastructure
{
    internal class BooruSearchEngine : ISearchEngine
    {
        private readonly IBooruAsyncLoader   _booruLoader;
        private readonly IBooruPostConverter _postConverter;

        public SearchEngineType SearchEngineType { get; }

        public BooruSearchEngine(
            IBooruAsyncLoader loader, 
            SearchEngineType searchEngineType,
            IBooruPostConverter postConverter)
        {
            _postConverter = postConverter;
            SearchEngineType = searchEngineType;
            _booruLoader = loader;
        }

        public async Task<SearchResult> Search(Image image)
        {
            var post = await FindPost(image.Md5);

            return post.SelectOrElse(
                x => _postConverter.Convert(x, image, SearchEngineType),
                () => Metadata.NotFound(image, SearchEngineType));
        }

        private async Task<Maybe<Post>> FindPost(string md5)
        {
            var searchResult = await _booruLoader.LoadSearchResultAsync($"md5:{md5}");

            if (!searchResult.NotEmpty)
                return Maybe<Post>.Nothing;
            
            var foundPost = searchResult.Results.First();
            var post = await _booruLoader.LoadPostAsync(foundPost.Id);

            return post.ToMaybe();
        }

        public interface IFactory
        {
            BooruSearchEngine Create(
                IBooruAsyncLoader loader,
                SearchEngineType  searchEngineType);
        }
    }
}