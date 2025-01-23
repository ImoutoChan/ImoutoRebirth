using Flurl.Http.Configuration;
using Imouto.BooruParser;
using Imouto.BooruParser.Implementations.Danbooru;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.Infrastructure.Abstract;
using ImoutoRebirth.Arachne.Infrastructure.Availability;
using Microsoft.Extensions.Options;
using DanbooruSettings = ImoutoRebirth.Arachne.Infrastructure.Models.Settings.DanbooruSettings;

namespace ImoutoRebirth.Arachne.Infrastructure.LoaderFabrics;

internal class DanbooruLoaderFabric : IBooruLoaderFabric
{
    private readonly DanbooruSettings _settings;
    private readonly IFlurlClientCache _flurlClientCache;

    public SearchEngineType ForType => SearchEngineType.Danbooru;

    public DanbooruLoaderFabric(DanbooruSettings settings, IFlurlClientCache flurlClientCache)
    {
        _settings = settings;
        _flurlClientCache = flurlClientCache;
    }

    public IBooruApiLoader Create() => new DanbooruApiLoader(
        _flurlClientCache,
        Options.Create(new Imouto.BooruParser.Implementations.Danbooru.DanbooruSettings()
        {
            ApiKey = _settings.ApiKey,
            Login = _settings.Login,
            PauseBetweenRequestsInMs = _settings.Delay,
            BotUserAgent = _settings.BotUserAgent
        }));

    public IBooruAvailabilityChecker CreateAvailabilityChecker()
        => new SimpleAvailabilityChecker(_flurlClientCache, new Uri("https://danbooru.donmai.us"));
}
