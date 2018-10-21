using System.Net.Http;
using Imouto.BooruParser.Loaders;
using ImoutoRebirth.Shop.Core.Models;
using ImoutoRebirth.Shop.Infrastructure.Abstract;

namespace ImoutoRebirth.Shop.Infrastructure.LoaderFabrics
{
    internal class YandereLoaderFabric : IBooruLoaderFabric
    {
        private readonly HttpClient _httpClient;

        public SearchEngineType ForType => SearchEngineType.Yandere;

        public YandereLoaderFabric(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public IBooruAsyncLoader Create() => new YandereLoader(_httpClient);
    }
}