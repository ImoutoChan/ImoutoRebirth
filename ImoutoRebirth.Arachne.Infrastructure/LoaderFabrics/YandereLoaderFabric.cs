using Imouto.BooruParser.Loaders;
using Imouto.BooruParser.Model.Danbooru;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.Infrastructure.Abstract;

namespace ImoutoRebirth.Arachne.Infrastructure.LoaderFabrics;

internal class YandereLoaderFabric : IBooruLoaderFabric
{
    private readonly HttpClient _httpClient;

    public SearchEngineType ForType => SearchEngineType.Yandere;

    public YandereLoaderFabric(HttpClient httpClient) => _httpClient = httpClient;

    public IBooruAsyncLoader Create() => new ExtendedYandereLoader(new YandereLoader(_httpClient));
}
