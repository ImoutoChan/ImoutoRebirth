using Flurl.Http.Configuration;
using Imouto.BooruParser;
using Imouto.BooruParser.Implementations.Gelbooru;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.Infrastructure.Abstract;
using ImoutoRebirth.Arachne.Infrastructure.Availability;
using Microsoft.Extensions.Options;
using GelbooruSettings = ImoutoRebirth.Arachne.Infrastructure.Models.Settings.GelbooruSettings;

namespace ImoutoRebirth.Arachne.Infrastructure.LoaderFabrics;

internal class GelbooruLoaderFabric : IBooruLoaderFabric, IAvailabilityProvider
{
    private readonly GelbooruSettings _settings;
    private readonly IFlurlClientCache _flurlClientCache;

    public SearchEngineType ForType => SearchEngineType.Gelbooru;

    public GelbooruLoaderFabric(GelbooruSettings settings, IFlurlClientCache flurlClientCache)
    {
        _settings = settings;
        _flurlClientCache = flurlClientCache;
    }

    public IBooruApiLoader Create() => new GelbooruApiLoader(
        _flurlClientCache,
        Options.Create(new Imouto.BooruParser.Implementations.Gelbooru.GelbooruSettings
        {
            ApiKey = _settings.ApiKey,
            UserId = int.TryParse(_settings.UserId, out var userId) ? userId : 0,
            PauseBetweenRequestsInMs = 1000
        }));

    public IAvailabilityChecker CreateAvailabilityChecker()
        => new SimpleAvailabilityChecker(_flurlClientCache, new Uri("https://gelbooru.com"));
}
