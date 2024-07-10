using Flurl.Http.Configuration;
using Imouto.BooruParser;
using Imouto.BooruParser.Implementations.Gelbooru;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.Infrastructure.Abstract;
using ImoutoRebirth.Arachne.Infrastructure.Availability;
using Microsoft.Extensions.Options;

namespace ImoutoRebirth.Arachne.Infrastructure.LoaderFabrics;

internal class GelbooruLoaderFabric : IBooruLoaderFabric
{
    private readonly IFlurlClientFactory _flurlClientFactory;

    public SearchEngineType ForType => SearchEngineType.Gelbooru;

    public GelbooruLoaderFabric(IFlurlClientFactory flurlClientFactory) => _flurlClientFactory = flurlClientFactory;

    public IBooruApiLoader Create() => new GelbooruApiLoader(
        _flurlClientFactory,
        Options.Create(new GelbooruSettings() { PauseBetweenRequestsInMs = 1 }));

    public IBooruAvailabilityChecker CreateAvailabilityChecker()
        => new SimpleAvailabilityChecker(_flurlClientFactory, new Uri("https://gelbooru.com"));
}
