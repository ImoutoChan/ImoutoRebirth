using Flurl.Http.Configuration;
using Imouto.BooruParser;
using Imouto.BooruParser.Implementations.Sankaku;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.Infrastructure.Abstract;
using Microsoft.Extensions.Options;
using SankakuSettings = ImoutoRebirth.Arachne.Infrastructure.Models.Settings.SankakuSettings;

namespace ImoutoRebirth.Arachne.Infrastructure.LoaderFabrics;

internal class SankakuLoaderFabric : IBooruLoaderFabric
{
    private readonly SankakuSettings _settings;
    private readonly ISankakuAuthManager _sankakuAuthManager;
    private readonly IFlurlClientFactory _flurlClientFactory;

    public SearchEngineType ForType => SearchEngineType.Sankaku;

    public SankakuLoaderFabric(
        SankakuSettings settings,
        ISankakuAuthManager sankakuAuthManager,
        IFlurlClientFactory flurlClientFactory)
    {
        _settings = settings;
        _sankakuAuthManager = sankakuAuthManager;
        _flurlClientFactory = flurlClientFactory;
    }
    
    public IBooruApiLoader Create() => new SankakuApiLoader(
        _flurlClientFactory,
        Options.Create(new Imouto.BooruParser.Implementations.Sankaku.SankakuSettings
        {
            PauseBetweenRequestsInMs = _settings.Delay,
            Login = _settings.Login,
            Password = _settings.Password
        }),
        _sankakuAuthManager);
}
