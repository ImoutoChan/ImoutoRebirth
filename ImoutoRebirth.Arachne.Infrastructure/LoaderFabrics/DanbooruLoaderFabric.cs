using Flurl.Http.Configuration;
using Imouto.BooruParser;
using Imouto.BooruParser.Implementations.Danbooru;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.Infrastructure.Abstract;
using Microsoft.Extensions.Options;
using DanbooruSettings = ImoutoRebirth.Arachne.Infrastructure.Models.Settings.DanbooruSettings;

namespace ImoutoRebirth.Arachne.Infrastructure.LoaderFabrics;

internal class DanbooruLoaderFabric : IBooruLoaderFabric
{
    private readonly DanbooruSettings _settings;
    private readonly IFlurlClientFactory _flurlClientFactory;

    public SearchEngineType ForType => SearchEngineType.Danbooru;

    public DanbooruLoaderFabric(DanbooruSettings settings, IFlurlClientFactory flurlClientFactory)
    {
        _settings = settings;
        _flurlClientFactory = flurlClientFactory;
    }

    public IBooruApiLoader Create() => new DanbooruApiLoader(
        _flurlClientFactory,
        Options.Create(new Imouto.BooruParser.Implementations.Danbooru.DanbooruSettings()
        {
            ApiKey = _settings.ApiKey,
            Login = _settings.Login,
            PauseBetweenRequestsInMs = _settings.Delay
        }));
}
