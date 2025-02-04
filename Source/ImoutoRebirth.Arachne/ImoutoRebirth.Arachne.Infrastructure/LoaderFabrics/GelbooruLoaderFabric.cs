using Flurl.Http.Configuration;
using Imouto.BooruParser;
using Imouto.BooruParser.Implementations.Gelbooru;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.Infrastructure.Abstract;
using ImoutoRebirth.Arachne.Infrastructure.Availability;
using Microsoft.Extensions.Options;

namespace ImoutoRebirth.Arachne.Infrastructure.LoaderFabrics;

internal class GelbooruLoaderFabric : IBooruLoaderFabric, IAvailabilityProvider
{
    private readonly IFlurlClientCache _flurlClientCache;

    public SearchEngineType ForType => SearchEngineType.Gelbooru;

    public GelbooruLoaderFabric(IFlurlClientCache flurlClientCache) => _flurlClientCache = flurlClientCache;

    public IBooruApiLoader Create() => new GelbooruApiLoader(
        _flurlClientCache,
        Options.Create(new GelbooruSettings() { PauseBetweenRequestsInMs = 1 }));

    public IAvailabilityChecker CreateAvailabilityChecker()
        => new SimpleAvailabilityChecker(_flurlClientCache, new Uri("https://gelbooru.com"));
}
