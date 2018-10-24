using System;
using System.Linq;
using System.Threading.Tasks;
using Imouto.BooruParser.Loaders;
using Imouto.BooruParser.Model.Base;
using ImoutoRebirth.Arachne.Core.InfrastructureContracts;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.Infrastructure.Abstract;
using Mackiovello.Maybe;
using SearchResult = ImoutoRebirth.Arachne.Core.Models.SearchResult;

namespace ImoutoRebirth.Arachne.Infrastructure
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
            try
            {
                var post = await FindPost(image.Md5);

                return post.SelectOrElse(
                    x => _postConverter.Convert(x, image, SearchEngineType),
                    () => Metadata.NotFound(image, SearchEngineType));
            }
            catch (Exception e)
            {
                return new SearchError(image, SearchEngineType, e.ToString());
            }
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