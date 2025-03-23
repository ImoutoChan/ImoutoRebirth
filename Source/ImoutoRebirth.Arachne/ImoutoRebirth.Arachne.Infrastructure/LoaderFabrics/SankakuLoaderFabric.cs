using Flurl.Http.Configuration;
using Imouto.BooruParser;
using Imouto.BooruParser.Implementations.Sankaku;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.Infrastructure.Abstract;
using ImoutoRebirth.Arachne.Infrastructure.Availability;
using Microsoft.Extensions.Options;
using SankakuSettings = ImoutoRebirth.Arachne.Infrastructure.Models.Settings.SankakuSettings;

namespace ImoutoRebirth.Arachne.Infrastructure.LoaderFabrics;

internal class SankakuLoaderFabric : IBooruLoaderFabric, IAvailabilityProvider
{
    private readonly SankakuSettings _settings;
    private readonly ISankakuAuthManager _sankakuAuthManager;
    private readonly IFlurlClientCache _flurlClientCache;

    public SearchEngineType ForType => SearchEngineType.Sankaku;

    public SankakuLoaderFabric(
        SankakuSettings settings,
        ISankakuAuthManager sankakuAuthManager,
        IFlurlClientCache flurlClientCache)
    {
        _settings = settings;
        _sankakuAuthManager = sankakuAuthManager;
        _flurlClientCache = flurlClientCache;
    }
    
    public IBooruApiLoader Create() => new SankakuApiLoader(
        _flurlClientCache,
        Options.Create(new Imouto.BooruParser.Implementations.Sankaku.SankakuSettings
        {
            PauseBetweenRequestsInMs = _settings.Delay,
            Login = _settings.Login,
            Password = _settings.Password
        }),
        _sankakuAuthManager);

    public IAvailabilityChecker CreateAvailabilityChecker()
        => new AlwaysUnavailableChecker();
        //=> new SimpleAvailabilityChecker(_flurlClientCache, new Uri("https://chan.sankakucomplex.com"));
}
