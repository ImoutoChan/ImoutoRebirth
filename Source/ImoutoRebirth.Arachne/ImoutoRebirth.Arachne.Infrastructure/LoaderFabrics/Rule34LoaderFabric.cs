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
    private readonly IFlurlClientFactory _flurlClientFactory;

    public SearchEngineType ForType => SearchEngineType.Rule34;

    public Rule34LoaderFabric(IFlurlClientFactory flurlClientFactory) => _flurlClientFactory = flurlClientFactory;

    public IBooruApiLoader Create() => new Rule34ApiLoader(
        _flurlClientFactory,
        Options.Create(new Rule34Settings { PauseBetweenRequestsInMs = 1 }));

    public IBooruAvailabilityChecker CreateAvailabilityChecker()
        => new SimpleAvailabilityChecker(_flurlClientFactory, new Uri("https://rule34.xxx"));
}
