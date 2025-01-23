using Flurl.Http.Configuration;
using Imouto.BooruParser;
using Imouto.BooruParser.Implementations.Yandere;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.Infrastructure.Abstract;
using ImoutoRebirth.Arachne.Infrastructure.Availability;
using Microsoft.Extensions.Options;

namespace ImoutoRebirth.Arachne.Infrastructure.LoaderFabrics;

internal class YandereLoaderFabric : IBooruLoaderFabric
{
    private readonly IFlurlClientCache _flurlClientCache;

    public SearchEngineType ForType => SearchEngineType.Yandere;

    public YandereLoaderFabric(IFlurlClientCache flurlClientCache) => _flurlClientCache = flurlClientCache;

    public IBooruApiLoader Create() => new YandereApiLoader(
        _flurlClientCache,
        Options.Create(new YandereSettings { PauseBetweenRequestsInMs = 0 }));

    public IBooruAvailabilityChecker CreateAvailabilityChecker()
        => new SimpleAvailabilityChecker(_flurlClientCache, new Uri("https://yande.re"));
}
