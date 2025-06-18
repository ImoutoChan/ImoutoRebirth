using System.Text.Json;

namespace ImoutoRebirth.Tori.Configuration;

public static class AppConfigurationExtensions
{
    public static AppConfiguration ReadAppConfiguration(this FileInfo globalConfigurationFile)
    {
        if (!globalConfigurationFile.Exists)
            throw new Exception("Unable to find global configuration file!");

        var configurationFileText = File.ReadAllText(globalConfigurationFile.FullName);

        var configurationDictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(configurationFileText)!;

        Migrate(globalConfigurationFile, configurationDictionary);
        ValidateConfigurationValues(configurationDictionary);

        return new()
        {
            Api = new()
            {
                DanbooruLogin = configurationDictionary["DanbooruLogin"],
                DanbooruApiKey = configurationDictionary["DanbooruApiKey"],
                SankakuLogin = configurationDictionary["SankakuLogin"],
                SankakuPassword = configurationDictionary["SankakuPassword"],
                YandereLogin = configurationDictionary["YandereLogin"],
                YandereApiKey = configurationDictionary["YandereApiKey"]
            },
            Connection = new()
            {
                LilinConnectionString = configurationDictionary["LilinConnectionString"],
                MeidoConnectionString = configurationDictionary["MeidoConnectionString"],
                RoomConnectionString = configurationDictionary["RoomConnectionString"],
                MassTransitConnectionString = configurationDictionary["MassTransitConnectionString"]
            },
            Ports = new()
            {
                RoomPort = configurationDictionary["RoomPort"],
                KekkaiPort = configurationDictionary["KekkaiPort"],
                LilinPort = configurationDictionary["LilinPort"]
            },
            Harpy = new()
            {
                SavePath = configurationDictionary["HarpySavePath"].Replace(@"\", @"\\"),
                FavoritesSaveJobRepeatEveryMinutes = configurationDictionary["HarpyFavoritesSaveJobRepeatEveryMinutes"]
            },
            Meido = new()
            {
                MetadataActualizerRepeatEveryMinutes = configurationDictionary["MeidoMetadataActualizerRepeatEveryMinutes"],
                FaultToleranceRepeatEveryMinutes = configurationDictionary["MeidoFaultToleranceRepeatEveryMinutes"],
                FaultToleranceIsEnabled = configurationDictionary["MeidoFaultToleranceIsEnabled"]
            },
            Room = new()
            {
                ImoutoPicsUploadUrl = configurationDictionary["RoomImoutoPicsUploadUrl"]
            },
            Kekkai = new()
            {
                AuthToken = configurationDictionary["KekkaiAuthToken"]
            },
            Jaeger = new()
            {
                Host = configurationDictionary["JaegerHost"],
                Port = configurationDictionary["JaegerPort"]
            },
            ExHentai = new()
            {
                IpbMemberId = configurationDictionary["ExHentaiIpbMemberId"],
                IpbPassHash = configurationDictionary["ExHentaiIpbPassHash"],
                Igneous = configurationDictionary["ExHentaiIgneous"],
                UserAgent = configurationDictionary["ExHentaiUserAgent"]
            },
            OpenSearchUri = configurationDictionary["OpenSearchUri"],
            InstallLocation = configurationDictionary["InstallLocation"]
        };
    }

    private static void ValidateConfigurationValues(Dictionary<string, string> configuration)
    {
        var keys = new[]
        {
            "DanbooruLogin",
            "DanbooruApiKey",
            "SankakuLogin",
            "SankakuPassword",
            "YandereLogin",
            "YandereApiKey",
            "OpenSearchUri",
            "RoomPort",
            "KekkaiPort",
            "LilinPort",
            "HarpySavePath",
            "HarpyFavoritesSaveJobRepeatEveryMinutes",
            "KekkaiAuthToken",
            "LilinConnectionString",
            "MeidoConnectionString",
            "RoomConnectionString",
            "MassTransitConnectionString",
            "MeidoMetadataActualizerRepeatEveryMinutes",
            "MeidoFaultToleranceRepeatEveryMinutes",
            "MeidoFaultToleranceIsEnabled",
            "RoomImoutoPicsUploadUrl",
            "JaegerHost",
            "JaegerPort",
            "ExHentaiIpbMemberId",
            "ExHentaiIpbPassHash",
            "ExHentaiIgneous",
            "ExHentaiUserAgent"
        };

        var missedKeys = keys.Where(x => !configuration.ContainsKey(x)).ToList();

        if (missedKeys.Any())
            throw new Exception("Missed configuration keys: " + string.Join(", ", missedKeys));
    }

    private static void Migrate(FileInfo globalConfigurationFile, Dictionary<string, string> configuration)
    {
        if (!configuration.ContainsKey("MassTransitConnectionString"))
        {
            // version 1 to version 2
            configuration.Add(
                "MassTransitConnectionString",
                "Server=localhost;Port=5432;Database=masstransit;User Id=postgres;Password=postgres;");

            configuration.Remove("RabbitMqUrl");
            configuration.Remove("RabbitMqUsername");
            configuration.Remove("RabbitMqPassword");

            File.WriteAllText(
                globalConfigurationFile.FullName,
                JsonSerializer.Serialize(configuration, new JsonSerializerOptions { WriteIndented = true }));
        }

        if (!configuration.ContainsKey("ExHentaiIpbMemberId"))
        {
            // version 2 to version 3, add exhentai options
            configuration.Add("ExHentaiIpbMemberId", "");
            configuration.Add("ExHentaiIpbPassHash", "");
            configuration.Add("ExHentaiIgneous", "");
            configuration.Add("ExHentaiUserAgent", "");

            File.WriteAllText(
                globalConfigurationFile.FullName,
                JsonSerializer.Serialize(configuration, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}

public class AppConfiguration
{
    public ApiSettings Api { get; set; } = new();
    public ConnectionSettings Connection { get; set; } = new();
    public PortsSettings Ports { get; set; } = new();
    public HarpySettings Harpy { get; set; } = new();
    public MeidoSettings Meido { get; set; } = new();
    public RoomSettings Room { get; set; } = new();
    public KekkaiSettings Kekkai { get; set; } = new();
    public JaegerSettings Jaeger { get; set; } = new();
    public ExHentaiSettings ExHentai { get; set; } = new();
    public string OpenSearchUri { get; set; } = string.Empty;
    public string InstallLocation { get; set; } = string.Empty;

    public class ApiSettings
    {
        public string DanbooruLogin { get; set; } = string.Empty;
        public string DanbooruApiKey { get; set; } = string.Empty;

        public string SankakuLogin { get; set; } = string.Empty;
        public string SankakuPassword { get; set; } = string.Empty;

        public string YandereLogin { get; set; } = string.Empty;
        public string YandereApiKey { get; set; } = string.Empty;
    }

    public class ConnectionSettings
    {
        public string LilinConnectionString { get; set; } = string.Empty;
        public string MeidoConnectionString { get; set; } = string.Empty;
        public string RoomConnectionString { get; set; } = string.Empty;
        public string MassTransitConnectionString { get; set; } = string.Empty;
    }

    public class PortsSettings
    {
        public string RoomPort { get; set; } = string.Empty;
        public string KekkaiPort { get; set; } = string.Empty;
        public string LilinPort { get; set; } = string.Empty;
    }

    public class HarpySettings
    {
        public string SavePath { get; set; } = string.Empty;
        public string FavoritesSaveJobRepeatEveryMinutes { get; set; } = string.Empty;
    }

    public class MeidoSettings
    {
        public string MetadataActualizerRepeatEveryMinutes { get; set; } = string.Empty;
        public string FaultToleranceRepeatEveryMinutes { get; set; } = string.Empty;
        public string FaultToleranceIsEnabled { get; set; } = string.Empty;
    }

    public class RoomSettings
    {
        public string ImoutoPicsUploadUrl { get; set; } = string.Empty;
    }

    public class KekkaiSettings
    {
        public string AuthToken { get; set; } = string.Empty;
    }

    public class JaegerSettings
    {
        public string Host { get; set; } = string.Empty;
        public string Port { get; set; } = string.Empty;
    }

    public class ExHentaiSettings
    {
        public string IpbMemberId { get; set; } = string.Empty;
        public string IpbPassHash { get; set; } = string.Empty;
        public string Igneous { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
    }
}
