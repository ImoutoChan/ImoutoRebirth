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
    public required ApiSettings Api { get; init; }
    public required ConnectionSettings Connection { get; init; }
    public required PortsSettings Ports { get; init; }
    public required HarpySettings Harpy { get; init; }
    public required MeidoSettings Meido { get; init; }
    public required RoomSettings Room { get; init; }
    public required KekkaiSettings Kekkai { get; init; }
    public required JaegerSettings Jaeger { get; init; }
    public required ExHentaiSettings ExHentai { get; init; }
    public required string OpenSearchUri { get; init; }
    public required string InstallLocation { get; init; }

    public class ApiSettings
    {
        public required string DanbooruLogin { get; init; }
        public required string DanbooruApiKey { get; init; }

        public required string SankakuLogin { get; init; }
        public required string SankakuPassword { get; init; }

        public required string YandereLogin { get; init; }
        public required string YandereApiKey { get; init; }
    }

    public class ConnectionSettings
    {
        public required string LilinConnectionString { get; init; }
        public required string MeidoConnectionString { get; init; }
        public required string RoomConnectionString { get; init; }
        public required string MassTransitConnectionString { get; init; }
    }

    public class PortsSettings
    {
        public required string RoomPort { get; init; }
        public required string KekkaiPort { get; init; }
        public required string LilinPort { get; init; }
    }

    public class HarpySettings
    {
        public required string SavePath { get; init; }
        public required string FavoritesSaveJobRepeatEveryMinutes { get; init; }
    }

    public class MeidoSettings
    {
        public required string MetadataActualizerRepeatEveryMinutes { get; init; }
        public required string FaultToleranceRepeatEveryMinutes { get; init; }
        public required string FaultToleranceIsEnabled { get; init; }
    }

    public class RoomSettings
    {
        public required string ImoutoPicsUploadUrl { get; init; }
    }

    public class KekkaiSettings
    {
        public required string AuthToken { get; init; }
    }

    public class JaegerSettings
    {
        public required string Host { get; init; }
        public required string Port { get; init; }
    }

    public class ExHentaiSettings
    {
        public required string IpbMemberId { get; init; }
        public required string IpbPassHash { get; init; }
        public required string Igneous { get; init; }
        public required string UserAgent { get; init; }
    }
}
