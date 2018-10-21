using System.Net.Http;
using Imouto.BooruParser.Loaders;
using ImoutoRebirth.Shop.Core.Models;
using ImoutoRebirth.Shop.Infrastructure.Abstract;
using ImoutoRebirth.Shop.Infrastructure.Models.Settings;

namespace ImoutoRebirth.Shop.Infrastructure.LoaderFabrics
{
    internal class DanbooruLoaderFabric : IBooruLoaderFabric
    {
        private readonly DanbooruSettings _settings;
        private readonly HttpClient _httpClient;

        public SearchEngineType ForType => SearchEngineType.Danbooru;

        public DanbooruLoaderFabric(HttpClient httpClient, DanbooruSettings settings)
        {
            _httpClient = httpClient;
            _settings = settings;
        }

        public IBooruAsyncLoader Create() 
            => new DanbooruLoader(
                _settings.Login, 
                _settings.ApiKey, 
                _settings.Delay, 
                _httpClient);
    }
}