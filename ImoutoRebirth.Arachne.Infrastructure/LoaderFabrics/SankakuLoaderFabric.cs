using System.Net.Http;
using Imouto.BooruParser.Loaders;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.Infrastructure.Abstract;
using ImoutoRebirth.Arachne.Infrastructure.Models.Settings;

namespace ImoutoRebirth.Arachne.Infrastructure.LoaderFabrics
{
    internal class SankakuLoaderFabric : IBooruLoaderFabric
    {
        private readonly SankakuSettings _settings;
        private readonly HttpClient      _httpClient;

        public SearchEngineType ForType => SearchEngineType.Sankaku;

        public SankakuLoaderFabric(HttpClient httpClient, SankakuSettings settings)
        {
            _httpClient = httpClient;
            _settings = settings;
        }

        public IBooruAsyncLoader Create() 
            => new SankakuLoader(
                _settings.Login, 
                _settings.PassHash, 
                _settings.Delay, 
                _httpClient);
    }
}