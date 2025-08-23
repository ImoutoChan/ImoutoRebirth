using System.Text.Json;

namespace ImoutoRebirth.Tori.Configuration;

public record AppConfiguration(
    AppConfiguration.ApiSettings Api,
    AppConfiguration.ConnectionSettings Connection,
    AppConfiguration.PortsSettings Ports,
    AppConfiguration.HarpySettings Harpy,
    AppConfiguration.MeidoSettings Meido,
    AppConfiguration.KekkaiSettings Kekkai,
    AppConfiguration.JaegerSettings Jaeger,
    AppConfiguration.ExHentaiSettings ExHentai,
    string OpenSearchUri,
    string InstallLocation,
    string FFmpegPath,
    bool WasMigrated)
{
    public record ApiSettings(
        string DanbooruLogin,
        string DanbooruApiKey,
        string SankakuLogin,
        string SankakuPassword,
        string YandereLogin,
        string YandereApiKey,
        string GelbooruUserId,
        string GelbooruApiKey,
        string Rule34UserId,
        string Rule34ApiKey);

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

    public static async Task<AppConfiguration> ReadFromFile(FileInfo globalConfigurationFile)
    {
        if (!globalConfigurationFile.Exists)
            throw new Exception("Unable to find global configuration file!");

        var configurationJson = await File.ReadAllTextAsync(globalConfigurationFile.FullName);
        var configurationDictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(configurationJson)!;

        var wasMigrated = await Migrate(globalConfigurationFile, configurationDictionary);
        return ReadFromDictionary(configurationDictionary, wasMigrated);
    }

    public async Task WriteToFile(FileInfo file)
    {
        var configurationDictionary = WriteToDictionary();
        ValidateConfigurationValues(configurationDictionary);

        await File.WriteAllTextAsync(
            file.FullName,
            JsonSerializer.Serialize(configurationDictionary, new JsonSerializerOptions { WriteIndented = true }));
    }

    public static AppConfiguration ReadFromDictionary(Dictionary<string,string> configurationDictionary, bool wasMigrated)
    {
        ValidateConfigurationValues(configurationDictionary);

        return new(
            Api: new(
                DanbooruLogin: configurationDictionary["DanbooruLogin"],
                DanbooruApiKey: configurationDictionary["DanbooruApiKey"],
                SankakuLogin: configurationDictionary["SankakuLogin"],
                SankakuPassword: configurationDictionary["SankakuPassword"],
                YandereLogin: configurationDictionary["YandereLogin"],
                YandereApiKey: configurationDictionary["YandereApiKey"],
                GelbooruUserId: configurationDictionary["GelbooruUserId"],
                GelbooruApiKey: configurationDictionary["GelbooruApiKey"],
                Rule34UserId: configurationDictionary["Rule34UserId"],
                Rule34ApiKey: configurationDictionary["Rule34ApiKey"]),
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
            InstallLocation: configurationDictionary["InstallLocation"],
            FFmpegPath: configurationDictionary["FFmpegPath"],
            WasMigrated: wasMigrated);
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



            ["KekkaiAuthToken"] = Kekkai.AuthToken,

            ["JaegerHost"] = Jaeger.Host,
            ["JaegerPort"] = Jaeger.Port,

            ["ExHentaiIpbMemberId"] = ExHentai.IpbMemberId,
            ["ExHentaiIpbPassHash"] = ExHentai.IpbPassHash,
            ["ExHentaiIgneous"] = ExHentai.Igneous,
            ["ExHentaiUserAgent"] = ExHentai.UserAgent,

            ["GelbooruUserId"] = Api.GelbooruUserId,
            ["GelbooruApiKey"] = Api.GelbooruApiKey,

            ["OpenSearchUri"] = OpenSearchUri,
            ["InstallLocation"] = InstallLocation,
            ["FFmpegPath"] = FFmpegPath,

            ["Rule34UserId"] = Api.Rule34UserId,
            ["Rule34ApiKey"] = Api.Rule34ApiKey,
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

            "JaegerHost",
            "JaegerPort",
            "ExHentaiIpbMemberId",
            "ExHentaiIpbPassHash",
            "ExHentaiIgneous",
            "ExHentaiUserAgent",
            "GelbooruUserId",
            "GelbooruApiKey",
            "FFmpegPath",
            "Rule34UserId",
            "Rule34ApiKey"
        };

        var missedKeys = keys.Where(x => !configuration.ContainsKey(x)).ToList();

        if (missedKeys.Any())
            throw new Exception("Missed configuration keys: " + string.Join(", ", missedKeys));
    }

    private static async Task<bool> Migrate(FileInfo globalConfigurationFile, Dictionary<string, string> configuration)
    {
        var wasMigrated = false;

        if (!configuration.ContainsKey("MassTransitConnectionString"))
        {
            // version 1 to version 2
            configuration.Add(
                "MassTransitConnectionString",
                "Server=localhost;Port=5432;Database=masstransit;User Id=postgres;Password=postgres;");

            configuration.Remove("RabbitMqUrl");
            configuration.Remove("RabbitMqUsername");
            configuration.Remove("RabbitMqPassword");

            await File.WriteAllTextAsync(
                globalConfigurationFile.FullName,
                JsonSerializer.Serialize(configuration, new JsonSerializerOptions { WriteIndented = true }));

            wasMigrated = true;
        }

        if (!configuration.ContainsKey("ExHentaiIpbMemberId"))
        {
            // version 2 to version 3, add exhentai options
            configuration.Add("ExHentaiIpbMemberId", "");
            configuration.Add("ExHentaiIpbPassHash", "");
            configuration.Add("ExHentaiIgneous", "");
            configuration.Add("ExHentaiUserAgent", "");

            await File.WriteAllTextAsync(
                globalConfigurationFile.FullName,
                JsonSerializer.Serialize(configuration, new JsonSerializerOptions { WriteIndented = true }));

            wasMigrated = true;
        }

        if (!configuration.ContainsKey("GelbooruUserId"))
        {
            // version 3 to version 4, add gelbooru options
            configuration.Add("GelbooruUserId", "");
            configuration.Add("GelbooruApiKey", "");

            await File.WriteAllTextAsync(
                globalConfigurationFile.FullName,
                JsonSerializer.Serialize(configuration, new JsonSerializerOptions { WriteIndented = true }));

            wasMigrated = true;
        }

        if (!configuration.ContainsKey("FFmpegPath"))
        {
            // version 4 to version 5, add ffmpeg path
            configuration.Add("FFmpegPath", "");

            await File.WriteAllTextAsync(
                globalConfigurationFile.FullName,
                JsonSerializer.Serialize(configuration, new JsonSerializerOptions { WriteIndented = true }));

            wasMigrated = true;
        }

        if (configuration.ContainsKey("RoomImoutoPicsUploadUrl"))
        {
            // version 5 to version 6, remove global upload url settings
            configuration.Remove("RoomImoutoPicsUploadUrl");

            await File.WriteAllTextAsync(
                globalConfigurationFile.FullName,
                JsonSerializer.Serialize(configuration, new JsonSerializerOptions { WriteIndented = true }));

            wasMigrated = true;
        }

        if (!configuration.ContainsKey("Rule34UserId"))
        {
            // version 6 to version 7, add rule34 options
            configuration.Add("Rule34UserId", "");
            configuration.Add("Rule34ApiKey", "");

            await File.WriteAllTextAsync(
                globalConfigurationFile.FullName,
                JsonSerializer.Serialize(configuration, new JsonSerializerOptions { WriteIndented = true }));

            wasMigrated = true;
        }

        return wasMigrated;
    }
}
