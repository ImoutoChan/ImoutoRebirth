using System.Text.Json;

namespace ImoutoRebirth.Tori.Services;

public interface IConfigurationBuilder
{
    void WriteProductionConfigurations(string newVersion);

    string GetInstallLocation();
}

public class ConfigurationBuilder : IConfigurationBuilder
{
    private readonly Dictionary<string, string> _configuration;
    private readonly string _danbooruApiKey;
    private readonly string _danbooruLogin;
    private readonly FileInfo _globalConfigurationFile;
    private readonly string _harpyFavoritesSaveJobRepeatEveryMinutes;
    private readonly string _harpySavePath;
    private readonly string _installLocation;
    private readonly string _kekkaiAuthToken;
    private readonly string _kekkaiPort;
    private readonly string _massTransitConnectionString;
    private readonly string _lilinConnectionString;
    private readonly string _lilinPort;
    private readonly string _meidoConnectionString;
    private readonly string _meidoFaultToleranceIsEnabled;
    private readonly string _meidoFaultToleranceRepeatEveryMinutes;
    private readonly string _meidoMetadataActualizerRepeatEveryMinutes;
    private readonly string _openSearchUri;
    private readonly string _roomConnectionString;
    private readonly string _roomImoutoPicsUploadUrl;
    private readonly string _roomPort;
    private readonly string _sankakuLogin;
    private readonly string _sankakuPassword;
    private readonly string _yandereApiKey;
    private readonly string _yandereLogin;
    private readonly string _jaegerHost;
    private readonly string _jaegerPort;
    private readonly string _exHentaiIpbMemberId;
    private readonly string _exHentaiIpbPassHash;
    private readonly string _exHentaiIgneous;
    private readonly string _exHentaiUserAgent;

    public ConfigurationBuilder(FileInfo globalConfigurationFile)
    {
        _globalConfigurationFile = globalConfigurationFile;

        if (!_globalConfigurationFile.Exists)
            throw new Exception("Unable to find global configuration file!");

        var configurationFileText = File.ReadAllText(_globalConfigurationFile.FullName);

        _configuration = JsonSerializer.Deserialize<Dictionary<string, string>>(configurationFileText)!;

        Migrate(_globalConfigurationFile, _configuration);
        
        ValidateConfigurationValues();

        _danbooruLogin = _configuration["DanbooruLogin"];
        _danbooruApiKey = _configuration["DanbooruApiKey"];
        _sankakuLogin = _configuration["SankakuLogin"];
        _sankakuPassword = _configuration["SankakuPassword"];
        _yandereLogin = _configuration["YandereLogin"];
        _yandereApiKey = _configuration["YandereApiKey"];

        _openSearchUri = _configuration["OpenSearchUri"];

        _roomPort = _configuration["RoomPort"];
        _kekkaiPort = _configuration["KekkaiPort"];
        _lilinPort = _configuration["LilinPort"];
        _lilinConnectionString = _configuration["LilinConnectionString"];
        _meidoConnectionString = _configuration["MeidoConnectionString"];
        _roomConnectionString = _configuration["RoomConnectionString"];
        _massTransitConnectionString = _configuration["MassTransitConnectionString"];

        _harpySavePath = _configuration["HarpySavePath"].Replace(@"\", @"\\");
        _harpyFavoritesSaveJobRepeatEveryMinutes = _configuration["HarpyFavoritesSaveJobRepeatEveryMinutes"];

        _kekkaiAuthToken = _configuration["KekkaiAuthToken"];

        _meidoMetadataActualizerRepeatEveryMinutes = _configuration["MeidoMetadataActualizerRepeatEveryMinutes"];
        _meidoFaultToleranceRepeatEveryMinutes = _configuration["MeidoFaultToleranceRepeatEveryMinutes"];
        _meidoFaultToleranceIsEnabled = _configuration["MeidoFaultToleranceIsEnabled"];

        _roomImoutoPicsUploadUrl = _configuration["RoomImoutoPicsUploadUrl"];
        _installLocation = _configuration["InstallLocation"];
        _jaegerHost = _configuration["JaegerHost"];
        _jaegerPort = _configuration["JaegerPort"];

        _exHentaiIpbMemberId = _configuration["ExHentaiIpbMemberId"];
        _exHentaiIpbPassHash = _configuration["ExHentaiIpbPassHash"];
        _exHentaiIgneous = _configuration["ExHentaiIgneous"];
        _exHentaiUserAgent = _configuration["ExHentaiUserAgent"];
    }

    private void Migrate(FileInfo globalConfigurationFile, Dictionary<string, string> configuration)
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

    public void WriteProductionConfigurations(string newVersion)
    {
        var serviceDirectories = _globalConfigurationFile.Directory!.GetDirectories();
        foreach (var serviceDirectory in serviceDirectories)
        {
            var configuration = serviceDirectory.Name switch
            {
                "ImoutoRebirth.Arachne" => GetArachneConfiguration(newVersion),
                "ImoutoRebirth.Harpy" => GetHarpyConfiguration(newVersion),
                "ImoutoRebirth.Kekkai" => GetKekkaiConfiguration(),
                "ImoutoRebirth.Lilin" => GetLilinConfiguration(),
                "ImoutoRebirth.Meido" => GetMeidoConfiguration(),
                "ImoutoRebirth.Room" => GetRoomConfiguration(),
                _ => null
            };

            if (configuration == null)
                continue;

            try
            {
                JsonSerializer.Deserialize<dynamic>(configuration);
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to parse built configuration for {serviceDirectory.Name}", e);
            }

            File.WriteAllText(Path.Combine(serviceDirectory.FullName, "appsettings.Production.json"), configuration);
        }
    }

    public string GetInstallLocation() => _installLocation;

    private void ValidateConfigurationValues()
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

        var missedKeys = keys.Where(x => !_configuration.ContainsKey(x)).ToList();

        if (missedKeys.Any())
            throw new Exception("Missed configuration keys: " + string.Join(", ", missedKeys));
    }

    private string GetArachneConfiguration(string currentVersion)
    {
        return
            $$"""
            {
              "ConnectionStrings": {
                "Masstransit": "{{_massTransitConnectionString}}"
              },
              "DanbooruSettings": {
                "Login": "{{_danbooruLogin}}",
                "ApiKey": "{{_danbooruApiKey}}",
                "Delay": "1",
                "BotUserAgent": "Arachne/{{currentVersion}}"
              },
              "SankakuSettings": {
                "Login": "{{_sankakuLogin}}",
                "Password": "{{_sankakuPassword}}",
                "Delay": "6000"
              },
              "ExHentaiSettings": {
                "IpbMemberId": "{{_exHentaiIpbMemberId}}",
                "IpbPassHash": "{{_exHentaiIpbPassHash}}",
                "Igneous": "{{_exHentaiIgneous}}",
                "UserAgent": "{{_exHentaiUserAgent}}"
              },
              "OpenSearchUri": "{{_openSearchUri}}",
              "Jaeger": {
                "Host": "{{_jaegerHost}}",
                "Port": {{_jaegerPort}}
              }
            }
            """;
    }

    private string GetHarpyConfiguration(string currentVersion)
    {
        return
            $$"""
            {
              "Danbooru": {
                "ApiKey": "{{_danbooruApiKey}}",
                "Login": "{{_danbooruLogin}}",
                "BotUserAgent": "Harpy/{{currentVersion}}"
              },
              "Yandere": {
                "ApiKey": "{{_yandereApiKey}}",
                "Login": "{{_yandereLogin}}"
              },
              "Saver": {
                "SaveToPath": "{{_harpySavePath}}",
                "RoomUrl": "http://localhost:{{_roomPort}}/"
              },
              "FavoritesSaveJobSettings": {
                "RepeatEveryMinutes": {{_harpyFavoritesSaveJobRepeatEveryMinutes}}
              },
              "OpenSearchUri": "{{_openSearchUri}}",
              "Jaeger": {
                "Host": "{{_jaegerHost}}",
                "Port": {{_jaegerPort}}
              }
            }
            """;
    }

    private string GetKekkaiConfiguration()
    {
        return
            $$"""
            {
              "AuthToken": "{{_kekkaiAuthToken}}",
              "OpenSearchUri": "{{_openSearchUri}}",
              "Kestrel": {
                "EndPoints": {
                  "Http": {
                    "Url": "http://*:{{_kekkaiPort}}"
                  }
                }
              },
              "Jaeger": {
                "Host": "{{_jaegerHost}}",
                "Port": {{_jaegerPort}}
              }
            }
            """;
    }

    private string GetLilinConfiguration()
    {
        return
            $$"""
            {
              "ConnectionStrings": {
                "LilinDatabase": "{{_lilinConnectionString}}",
                "Masstransit": "{{_massTransitConnectionString}}"
              },
              "OpenSearchUri": "{{_openSearchUri}}",
              "Kestrel": {
                "EndPoints": {
                  "Http": {
                    "Url": "http://*:{{_lilinPort}}"
                  }
                }
              },
              "Jaeger": {
                "Host": "{{_jaegerHost}}",
                "Port": {{_jaegerPort}}
              }
            }
            """;
    }

    private string GetMeidoConfiguration()
    {
        return
            $$"""
            {
              "ConnectionStrings": {
                "MeidoDatabase": "{{_meidoConnectionString}}",
                "Masstransit": "{{_massTransitConnectionString}}"
              },
              "OpenSearchUri": "{{_openSearchUri}}",
              "MetadataActualizerSettings": {
                "RepeatEveryMinutes": {{_meidoMetadataActualizerRepeatEveryMinutes}},
                "ActiveSources": [
                  "Yandere", "Danbooru"
                ] 
              },
              "FaultToleranceSettings": {
                "RepeatEveryMinutes": {{_meidoFaultToleranceRepeatEveryMinutes}},
                "IsEnabled": {{_meidoFaultToleranceIsEnabled}}
              },
              "Jaeger": {
                "Host": "{{_jaegerHost}}",
                "Port": {{_jaegerPort}}
              }
            }
            """;
    }

    private string GetRoomConfiguration()
    {
        return
            $$"""
            {
              "Kestrel": {
                "EndPoints": {
                  "Http": {
                    "Url": "http://*:{{_roomPort}}"
                  }
                }
              },
              "ConnectionStrings": {
                "RoomDatabase": "{{_roomConnectionString}}",
                "Masstransit": "{{_massTransitConnectionString}}"
              },
              "OpenSearchUri": "{{_openSearchUri}}",
              "ImoutoPicsUploadUrl": "{{_roomImoutoPicsUploadUrl}}",
              "Jaeger": {
                "Host": "{{_jaegerHost}}",
                "Port": {{_jaegerPort}}
              }
            }
            """;
    }
}
