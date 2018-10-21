using System.Net.Http;
using Imouto.BooruParser.Loaders;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.Infrastructure.Abstract;
using ImoutoRebirth.Arachne.Infrastructure.Models.Settings;

namespace ImoutoRebirth.Arachne.Infrastructure.LoaderFabrics
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