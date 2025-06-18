using System.Text.Json;

namespace ImoutoRebirth.Tori.Configuration;

public record AppConfiguration(
    AppConfiguration.ApiSettings Api,
    AppConfiguration.ConnectionSettings Connection,
    AppConfiguration.PortsSettings Ports,
    AppConfiguration.HarpySettings Harpy,
    AppConfiguration.MeidoSettings Meido,
    AppConfiguration.RoomSettings Room,
    AppConfiguration.KekkaiSettings Kekkai,
    AppConfiguration.JaegerSettings Jaeger,
    AppConfiguration.ExHentaiSettings ExHentai,
    string OpenSearchUri,
    string InstallLocation)
{
    public record ApiSettings(
        string DanbooruLogin,
        string DanbooruApiKey,
        string SankakuLogin,
        string SankakuPassword,
        string YandereLogin,
        string YandereApiKey);

    public record ConnectionSettings(
        string LilinConnectionString,
        string MeidoConnectionString,
        string RoomConnectionString,
        string MassTransitConnectionString);

    public record PortsSettings(
        string RoomPort,
        string KekkaiPort,
        string LilinPort);

    public record HarpySettings(
        string SavePath,
        string FavoritesSaveJobRepeatEveryMinutes);

    public record MeidoSettings(
        string MetadataActualizerRepeatEveryMinutes,
        string FaultToleranceRepeatEveryMinutes,
        string FaultToleranceIsEnabled);

    public record RoomSettings(
        string ImoutoPicsUploadUrl);

    public record KekkaiSettings(
        string AuthToken);

    public record JaegerSettings(
        string Host,
        string Port);

    public record ExHentaiSettings(
        string IpbMemberId,
        string IpbPassHash,
        string Igneous,
        string UserAgent);

    public static AppConfiguration ReadFromFile(FileInfo globalConfigurationFile)
    {
        if (!globalConfigurationFile.Exists)
            throw new Exception("Unable to find global configuration file!");

        var configurationJson = File.ReadAllText(globalConfigurationFile.FullName);
        var configurationDictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(configurationJson)!;

        Migrate(globalConfigurationFile, configurationDictionary);
        return ReadFromDictionary(configurationDictionary);
    }

    public void WriteToFile(FileInfo file)
    {
        var configurationDictionary = WriteToDictionary();
        ValidateConfigurationValues(configurationDictionary);

        File.WriteAllText(file.FullName, JsonSerializer.Serialize(configurationDictionary));
    }

    public static AppConfiguration ReadFromDictionary(Dictionary<string,string> configurationDictionary)
    {
        ValidateConfigurationValues(configurationDictionary);

        return new(
            Api: new(
                DanbooruLogin: configurationDictionary["DanbooruLogin"],
                DanbooruApiKey: configurationDictionary["DanbooruApiKey"],
                SankakuLogin: configurationDictionary["SankakuLogin"],
                SankakuPassword: configurationDictionary["SankakuPassword"],
                YandereLogin: configurationDictionary["YandereLogin"],
                YandereApiKey: configurationDictionary["YandereApiKey"]),
            Connection: new(
                LilinConnectionString: configurationDictionary["LilinConnectionString"],
                MeidoConnectionString: configurationDictionary["MeidoConnectionString"],
                RoomConnectionString: configurationDictionary["RoomConnectionString"],
                MassTransitConnectionString: configurationDictionary["MassTransitConnectionString"]),
            Ports: new(
                RoomPort: configurationDictionary["RoomPort"],
                KekkaiPort: configurationDictionary["KekkaiPort"],
                LilinPort: configurationDictionary["LilinPort"]),
            Harpy: new(
                SavePath: configurationDictionary["HarpySavePath"].Replace(@"\", @"\\"),
                FavoritesSaveJobRepeatEveryMinutes: configurationDictionary["HarpyFavoritesSaveJobRepeatEveryMinutes"]),
            Meido: new(
                MetadataActualizerRepeatEveryMinutes: configurationDictionary["MeidoMetadataActualizerRepeatEveryMinutes"],
                FaultToleranceRepeatEveryMinutes: configurationDictionary["MeidoFaultToleranceRepeatEveryMinutes"],
                FaultToleranceIsEnabled: configurationDictionary["MeidoFaultToleranceIsEnabled"]),
            Room: new(
                ImoutoPicsUploadUrl: configurationDictionary["RoomImoutoPicsUploadUrl"]),
            Kekkai: new(
                AuthToken: configurationDictionary["KekkaiAuthToken"]),
            Jaeger: new(
                Host: configurationDictionary["JaegerHost"],
                Port: configurationDictionary["JaegerPort"]),
            ExHentai: new(
                IpbMemberId: configurationDictionary["ExHentaiIpbMemberId"],
                IpbPassHash: configurationDictionary["ExHentaiIpbPassHash"],
                Igneous: configurationDictionary["ExHentaiIgneous"],
                UserAgent: configurationDictionary["ExHentaiUserAgent"]),
            OpenSearchUri: configurationDictionary["OpenSearchUri"],
            InstallLocation: configurationDictionary["InstallLocation"]);
    }

    public Dictionary<string, string> WriteToDictionary()
        => new()
        {
            ["DanbooruLogin"] = Api.DanbooruLogin,
            ["DanbooruApiKey"] = Api.DanbooruApiKey,
            ["SankakuLogin"] = Api.SankakuLogin,
            ["SankakuPassword"] = Api.SankakuPassword,
            ["YandereLogin"] = Api.YandereLogin,
            ["YandereApiKey"] = Api.YandereApiKey,

            ["LilinConnectionString"] = Connection.LilinConnectionString,
            ["MeidoConnectionString"] = Connection.MeidoConnectionString,
            ["RoomConnectionString"] = Connection.RoomConnectionString,
            ["MassTransitConnectionString"] = Connection.MassTransitConnectionString,

            ["RoomPort"] = Ports.RoomPort,
            ["KekkaiPort"] = Ports.KekkaiPort,
            ["LilinPort"] = Ports.LilinPort,

            ["HarpySavePath"] = Harpy.SavePath.Replace(@"\\", @"\"),
            ["HarpyFavoritesSaveJobRepeatEveryMinutes"] = Harpy.FavoritesSaveJobRepeatEveryMinutes,

            ["MeidoMetadataActualizerRepeatEveryMinutes"] = Meido.MetadataActualizerRepeatEveryMinutes,
            ["MeidoFaultToleranceRepeatEveryMinutes"] = Meido.FaultToleranceRepeatEveryMinutes,
            ["MeidoFaultToleranceIsEnabled"] = Meido.FaultToleranceIsEnabled,

            ["RoomImoutoPicsUploadUrl"] = Room.ImoutoPicsUploadUrl,

            ["KekkaiAuthToken"] = Kekkai.AuthToken,

            ["JaegerHost"] = Jaeger.Host,
            ["JaegerPort"] = Jaeger.Port,

            ["ExHentaiIpbMemberId"] = ExHentai.IpbMemberId,
            ["ExHentaiIpbPassHash"] = ExHentai.IpbPassHash,
            ["ExHentaiIgneous"] = ExHentai.Igneous,
            ["ExHentaiUserAgent"] = ExHentai.UserAgent,

            ["OpenSearchUri"] = OpenSearchUri,
            ["InstallLocation"] = InstallLocation
        };

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
