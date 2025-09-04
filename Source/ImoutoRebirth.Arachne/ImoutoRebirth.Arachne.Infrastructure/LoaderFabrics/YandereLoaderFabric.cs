using Flurl.Http.Configuration;
using Imouto.BooruParser;
using Imouto.BooruParser.Implementations.Yandere;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.Infrastructure.Abstract;
using ImoutoRebirth.Arachne.Infrastructure.Availability;
using Microsoft.Extensions.Options;
using YandereSettings = ImoutoRebirth.Arachne.Infrastructure.Models.Settings.YandereSettings;

namespace ImoutoRebirth.Arachne.Infrastructure.LoaderFabrics;

internal class YandereLoaderFabric : IBooruLoaderFabric, IAvailabilityProvider
{
    private readonly IFlurlClientCache _flurlClientCache;
    private readonly YandereSettings _settings;

    public SearchEngineType ForType => SearchEngineType.Yandere;

    public YandereLoaderFabric(IFlurlClientCache flurlClientCache, YandereSettings settings)
    {
        _flurlClientCache = flurlClientCache;
        _settings = settings;
    }

    public IBooruApiLoader Create() => new YandereApiLoader(
        _flurlClientCache,
        Options.Create(new Imouto.BooruParser.Implementations.Yandere.YandereSettings()
        {
            PauseBetweenRequestsInMs = _settings.Delay,
            BotUserAgent = _settings.BotUserAgent
        }));

    public IAvailabilityChecker CreateAvailabilityChecker()
        => new SimpleAvailabilityChecker(_flurlClientCache, new Uri("https://yande.re"));
}
