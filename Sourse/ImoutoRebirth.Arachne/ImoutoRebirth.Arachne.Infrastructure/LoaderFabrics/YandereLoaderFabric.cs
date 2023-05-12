using Flurl.Http.Configuration;
using Imouto.BooruParser;
using Imouto.BooruParser.Implementations.Yandere;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.Infrastructure.Abstract;
using Microsoft.Extensions.Options;

namespace ImoutoRebirth.Arachne.Infrastructure.LoaderFabrics;

internal class YandereLoaderFabric : IBooruLoaderFabric
{
    private readonly IFlurlClientFactory _flurlClientFactory;

    public SearchEngineType ForType => SearchEngineType.Yandere;

    public YandereLoaderFabric(IFlurlClientFactory flurlClientFactory) => _flurlClientFactory = flurlClientFactory;

    public IBooruApiLoader Create() => new YandereApiLoader(
        _flurlClientFactory,
        Options.Create(new YandereSettings { PauseBetweenRequestsInMs = 0 }));
}
