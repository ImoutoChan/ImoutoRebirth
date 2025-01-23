using Flurl.Http.Configuration;
using Imouto.BooruParser;
using Imouto.BooruParser.Implementations.Rule34;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.Infrastructure.Abstract;
using ImoutoRebirth.Arachne.Infrastructure.Availability;
using Microsoft.Extensions.Options;

namespace ImoutoRebirth.Arachne.Infrastructure.LoaderFabrics;

internal class Rule34LoaderFabric : IBooruLoaderFabric
{
    private readonly IFlurlClientCache _flurlClientCache;

    public SearchEngineType ForType => SearchEngineType.Rule34;

    public Rule34LoaderFabric(IFlurlClientCache flurlClientCache) => _flurlClientCache = flurlClientCache;

    public IBooruApiLoader Create() => new Rule34ApiLoader(
        _flurlClientCache,
        Options.Create(new Rule34Settings { PauseBetweenRequestsInMs = 1 }));

    public IBooruAvailabilityChecker CreateAvailabilityChecker()
        => new SimpleAvailabilityChecker(_flurlClientCache, new Uri("https://rule34.xxx"));
}
