using System.Globalization;
using System.Text.Json;

namespace ImoutoRebirth.Tori.Configuration;

public interface IConfigurationBuilder
{
    Task WriteProductionConfigurations(string newVersion, DirectoryInfo updaterLocation);

    DirectoryInfo GetInstallLocation();
}

public class ConfigurationBuilder : IConfigurationBuilder
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly AppConfiguration _configuration;

    public ConfigurationBuilder(AppConfiguration configuration)
        => _configuration = configuration;

    public async Task<ConfigurationBuilder> CreateFromFile(FileInfo globalConfigurationFile)
        => new(await AppConfiguration.ReadFromFile(globalConfigurationFile));

    public async Task WriteProductionConfigurations(string newVersion, DirectoryInfo updaterLocation)
    {
        var serviceDirectories = updaterLocation.GetDirectories();
        foreach (var serviceDirectory in serviceDirectories)
        {
            object? configuration = serviceDirectory.Name switch
            {
                "Imouto.Viewer" => new
                {
                    RoomUrl = $"http://localhost:{_configuration.Ports.RoomPort}/",
                    LilinUrl = $"http://localhost:{_configuration.Ports.LilinPort}/"
                },
                "ImoutoRebirth.Arachne" => GetArachneConfiguration(newVersion),
                "ImoutoRebirth.Harpy" => GetHarpyConfiguration(newVersion),
                "ImoutoRebirth.Kekkai" => GetKekkaiConfiguration(),
                "ImoutoRebirth.Lilin" => GetLilinConfiguration(),
                "ImoutoRebirth.Meido" => GetMeidoConfiguration(),
                "ImoutoRebirth.Room" => GetRoomConfiguration(),
                "ImoutoRebirth.Lamia" => GetLamiaConfiguration(),
                _ => null
            };

            if (configuration == null)
                continue;

            await File.WriteAllTextAsync(
                Path.Combine(serviceDirectory.FullName, "appsettings.Production.json"),
                JsonSerializer.Serialize(configuration, JsonOptions));
        }
    }

    public DirectoryInfo GetInstallLocation() => new(_configuration.InstallLocation);

    public AppConfiguration GetCurrentConfiguration() => _configuration;

    private object GetArachneConfiguration(string currentVersion)
        => new
        {
            ConnectionStrings = new { MassTransit = _configuration.Connection.MassTransitConnectionString },
            DanbooruSettings = new
            {
                Login = _configuration.Api.DanbooruLogin,
                ApiKey = _configuration.Api.DanbooruApiKey,
                UserId = _configuration.Api.DanbooruUserId,
                Delay = "1000",
                BotUserAgent = $"Arachne/{currentVersion}"
            },
            SankakuSettings = new
            {
                Login = _configuration.Api.SankakuLogin,
                Password = _configuration.Api.SankakuPassword,
                Delay = "6000"
            },
            ExHentaiSettings = _configuration.ExHentai,
            GelbooruSettings = new { UserId = _configuration.Api.GelbooruUserId, ApiKey = _configuration.Api.GelbooruApiKey },
            Rule34Settings = new
            {
                UserId = _configuration.Api.Rule34UserId,
                ApiKey = _configuration.Api.Rule34ApiKey,
                BotUserAgent = $"Arachne/{currentVersion}"
            },
            YandereSettings = new { Delay = 0, BotUserAgent = $"Arachne/{currentVersion}" },
            _configuration.OpenSearchUri,
            Jaeger
        };

    private object GetHarpyConfiguration(string currentVersion)
        => new
        {
            Danbooru = new
            {
                ApiKey = _configuration.Api.DanbooruApiKey,
                Login = _configuration.Api.DanbooruLogin,
                UserId = _configuration.Api.DanbooruUserId,
                BotUserAgent = $"Harpy/{currentVersion}"
            },
            Yandere = new
            {
                ApiKey = _configuration.Api.YandereApiKey,
                Login = _configuration.Api.YandereLogin,
                BotUserAgent = $"Harpy/{currentVersion}"
            },
            Gelbooru = new { UserId = _configuration.Api.GelbooruUserId, ApiKey = _configuration.Api.GelbooruApiKey },
            Saver = new
            {
                SaveToPath = _configuration.Harpy.SavePath,
                RoomUrl = $"http://localhost:{_configuration.Ports.RoomPort}/"
            },
            FavoritesSaveJobSettings = new
            {
                RepeatEveryMinutes = int.Parse(_configuration.Harpy.FavoritesSaveJobRepeatEveryMinutes, CultureInfo.InvariantCulture)
            },
            _configuration.OpenSearchUri,
            Jaeger
        };

    private object GetKekkaiConfiguration()
        => new
        {
            _configuration.Kekkai.AuthToken,
            _configuration.OpenSearchUri,
            LilinUrl = $"http://localhost:{_configuration.Ports.LilinPort}",
            RoomUrl = $"http://localhost:{_configuration.Ports.RoomPort}",
            Kestrel = Kestrel(_configuration.Ports.KekkaiPort),
            Jaeger
        };

    private object GetLilinConfiguration()
        => new
        {
            ConnectionStrings = new
            {
                LilinDatabase = _configuration.Connection.LilinConnectionString,
                MassTransit = _configuration.Connection.MassTransitConnectionString
            },
            RecalculateTagCountersSettings = new { RepeatEveryMinutes = 15 },
            _configuration.OpenSearchUri,
            Kestrel = Kestrel(_configuration.Ports.LilinPort),
            Jaeger
        };

    private object GetMeidoConfiguration()
        => new
        {
            ConnectionStrings = new
            {
                MeidoDatabase = _configuration.Connection.MeidoConnectionString,
                MassTransit = _configuration.Connection.MassTransitConnectionString
            },
            _configuration.OpenSearchUri,
            MetadataActualizerSettings = new
            {
                RepeatEveryMinutes = int.Parse(_configuration.Meido.MetadataActualizerRepeatEveryMinutes, CultureInfo.InvariantCulture),
                ActiveSources = new[] { "Yandere", "Danbooru" }
            },
            FaultToleranceSettings = new
            {
                RepeatEveryMinutes = int.Parse(_configuration.Meido.FaultToleranceRepeatEveryMinutes, CultureInfo.InvariantCulture),
                IsEnabled = bool.Parse(_configuration.Meido.FaultToleranceIsEnabled)
            },
            Jaeger
        };

    private object GetLamiaConfiguration()
        => new
        {
            ConnectionStrings = new { MassTransit = _configuration.Connection.MassTransitConnectionString },
            _configuration.OpenSearchUri,
            Jaeger,
            FFmpeg = new { Path = _configuration.FFmpegPath }
        };

    private object GetRoomConfiguration()
        => new
        {
            Kestrel = Kestrel(_configuration.Ports.RoomPort),
            ConnectionStrings = new
            {
                RoomDatabase = _configuration.Connection.RoomConnectionString,
                MassTransit = _configuration.Connection.MassTransitConnectionString
            },
            _configuration.OpenSearchUri,
            Jaeger
        };

    private object Jaeger => new
    {
        _configuration.Jaeger.Host,
        Port = int.Parse(_configuration.Jaeger.Port, CultureInfo.InvariantCulture)
    };

    private static object Kestrel(string port)
        => new { EndPoints = new { Http = new { Url = $"http://*:{port}" } } };
}
