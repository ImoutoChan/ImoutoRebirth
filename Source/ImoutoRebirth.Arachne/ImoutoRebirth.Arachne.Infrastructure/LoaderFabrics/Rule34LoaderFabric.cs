using Flurl.Http.Configuration;
using Imouto.BooruParser;
using Imouto.BooruParser.Implementations.Rule34;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.Infrastructure.Abstract;
using ImoutoRebirth.Arachne.Infrastructure.Availability;
using Microsoft.Extensions.Options;
using Rule34Settings = ImoutoRebirth.Arachne.Infrastructure.Models.Settings.Rule34Settings;

namespace ImoutoRebirth.Arachne.Infrastructure.LoaderFabrics;

internal class Rule34LoaderFabric : IBooruLoaderFabric, IAvailabilityProvider
{
    private readonly Rule34Settings _settings;
    private readonly IFlurlClientCache _flurlClientCache;

    public SearchEngineType ForType => SearchEngineType.Rule34;

    public Rule34LoaderFabric(Rule34Settings settings, IFlurlClientCache flurlClientCache)
    {
        _settings = settings;
        _flurlClientCache = flurlClientCache;
    }

    public IBooruApiLoader Create() => new Rule34ApiLoader(
        _flurlClientCache,
        Options.Create(new Imouto.BooruParser.Implementations.Rule34.Rule34Settings
        {
            ApiKey = _settings.ApiKey,
            UserId = int.TryParse(_settings.UserId, out var userId) ? userId : 0,
            PauseBetweenRequestsInMs = 1000
        }));

    public IAvailabilityChecker CreateAvailabilityChecker()
        => new SimpleAvailabilityChecker(_flurlClientCache, new Uri("https://rule34.xxx"));
}
