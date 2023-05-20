﻿using System.Text.Json;

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
    private readonly string _elasticUrl;
    private readonly FileInfo _globalConfigurationFile;
    private readonly string _harpyFavoritesSaveJobRepeatEveryMinutes;
    private readonly string _harpySavePath;
    private readonly string _installLocation;
    private readonly string _kekkaiAuthToken;
    private readonly string _kekkaiPort;
    private readonly string _lilinConnectionString;
    private readonly string _lilinPort;
    private readonly string _meidoConnectionString;
    private readonly string _meidoFaultToleranceIsEnabled;
    private readonly string _meidoFaultToleranceRepeatEveryMinutes;
    private readonly string _meidoMetadataActualizerRepeatEveryMinutes;
    private readonly string _rabbitPassword;
    private readonly string _rabbitUrl;
    private readonly string _rabbitUsername;
    private readonly string _roomConnectionString;
    private readonly string _roomImoutoPicsUploadUrl;
    private readonly string _roomPort;
    private readonly string _sankakuLogin;
    private readonly string _sankakuPassHash;
    private readonly string _yandereApiKey;
    private readonly string _yandereLogin;

    public ConfigurationBuilder(FileInfo globalConfigurationFile)
    {
        _globalConfigurationFile = globalConfigurationFile;

        if (!_globalConfigurationFile.Exists)
            throw new Exception("Unable to find global configuration file!");

        var configurationFileText = _globalConfigurationFile.OpenText().ReadToEnd();

        _configuration = JsonSerializer.Deserialize<Dictionary<string, string>>(configurationFileText)!;

        ValidateConfigurationValues();

        _rabbitUrl = _configuration["RabbitMqUrl"];
        _rabbitUsername = _configuration["RabbitMqUsername"];
        _rabbitPassword = _configuration["RabbitMqPassword"];

        _danbooruLogin = _configuration["DanbooruLogin"];
        _danbooruApiKey = _configuration["DanbooruApiKey"];
        _sankakuLogin = _configuration["SankakuLogin"];
        _sankakuPassHash = _configuration["SankakuPassHash"];
        _yandereLogin = _configuration["YandereLogin"];
        _yandereApiKey = _configuration["YandereApiKey"];

        _elasticUrl = _configuration["ElasticUrl"];

        _roomPort = _configuration["RoomPort"];
        _kekkaiPort = _configuration["KekkaiPort"];
        _lilinPort = _configuration["LilinPort"];
        _lilinConnectionString = _configuration["LilinConnectionString"];
        _meidoConnectionString = _configuration["MeidoConnectionString"];
        _roomConnectionString = _configuration["RoomConnectionString"];

        _harpySavePath = _configuration["HarpySavePath"];
        _harpyFavoritesSaveJobRepeatEveryMinutes = _configuration["HarpyFavoritesSaveJobRepeatEveryMinutes"];

        _kekkaiAuthToken = _configuration["KekkaiAuthToken"];

        _meidoMetadataActualizerRepeatEveryMinutes = _configuration["MeidoMetadataActualizerRepeatEveryMinutes"];
        _meidoFaultToleranceRepeatEveryMinutes = _configuration["MeidoFaultToleranceRepeatEveryMinutes"];
        _meidoFaultToleranceIsEnabled = _configuration["MeidoFaultToleranceIsEnabled"];

        _roomImoutoPicsUploadUrl = _configuration["RoomImoutoPicsUploadUrl"];
        _installLocation = _configuration["InstallLocation"];
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

            File.WriteAllText(Path.Combine(serviceDirectory.FullName, "appsettings.Production.json"), configuration);
        }
    }

    public string GetInstallLocation() => _installLocation;

    private void ValidateConfigurationValues()
    {
        var keys = new[]
        {
            "RabbitMqUrl",
            "RabbitMqUsername",
            "RabbitMqPassword",
            "DanbooruLogin",
            "DanbooruApiKey",
            "SankakuLogin",
            "SankakuPassHash",
            "YandereLogin",
            "YandereApiKey",
            "ElasticUrl",
            "RoomPort",
            "KekkaiPort",
            "LilinPort",
            "HarpySavePath",
            "HarpyFavoritesSaveJobRepeatEveryMinutes",
            "KekkaiAuthToken",
            "LilinConnectionString",
            "MeidoConnectionString",
            "RoomConnectionString",
            "MeidoMetadataActualizerRepeatEveryMinutes",
            "MeidoFaultToleranceRepeatEveryMinutes",
            "MeidoFaultToleranceIsEnabled",
            "RoomImoutoPicsUploadUrl"
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
              "DanbooruSettings": {
                "Login": "{{_danbooruLogin}}",
                "ApiKey": "{{_danbooruApiKey}}",
                "Delay": "1",
                "BotUserAgent": "Arachne/{{currentVersion}}"
              },
              "SankakuSettings": {
                "Login": "{{_sankakuLogin}}",
                "PassHash": "{{_sankakuPassHash}}",
                "Delay": "6000"
              },
              "RabbitSettings": {
                "Url": "{{_rabbitUrl}}",
                "Username": "{{_rabbitUsername}}",
                "Password": "{{_rabbitPassword}}"
              },
              "Serilog": {
                "WriteTo": [
                  {
                    "Name": "Elasticsearch",
                    "Args": {
                      "nodeUris": "{{_elasticUrl}}",
                      "indexFormat": "imoutorebirth-arachne-{0:yyyy.MM}",
                      "restrictedToMinimumLevel": "Information"
                    }
                  }
                ],
                "Properties": {
                  "Application": "ImoutoRebirth.Arachne"
                }
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
              "Serilog": {
                "WriteTo": [
                  {
                    "Name": "Elasticsearch",
                    "Args": {
                      "nodeUris": "{{_elasticUrl}}",
                      "indexFormat": "imoutorebirth-harpy-{0:yyyy.MM}",
                      "restrictedToMinimumLevel": "Information"
                    }
                  }
                ],
                "Properties": {
                  "Application": "ImoutoRebirth.Harpy"
                }
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
              "RabbitSettings": {
                "Url": "{{_rabbitUrl}}",
                "Username": "{{_rabbitUsername}}",
                "Password": "{{_rabbitPassword}}"
              },
              "Serilog": {
                "WriteTo": [
                  {
                    "Name": "Elasticsearch",
                    "Args": {
                      "nodeUris": "{{_elasticUrl}}",
                      "indexFormat": "imoutorebirth-kekkai-{0:yyyy.MM}",
                      "restrictedToMinimumLevel": "Information"
                    }
                  }
                ],
                "Properties": {
                  "Application": "ImoutoRebirth.Kekkai"
                }
              },
              "Kestrel": {
                "EndPoints": {
                  "Http": {
                    "Url": "http://*:{{_kekkaiPort}}"
                  }
                }
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
                "LilinDatabase": "{{_lilinConnectionString}}"
              },
              "RabbitSettings": {
                "Url": "{{_rabbitUrl}}",
                "Username": "{{_rabbitUsername}}",
                "Password": "{{_rabbitPassword}}"
              },
              "Serilog": {
                "WriteTo": [
                  {
                    "Name": "Elasticsearch",
                    "Args": {
                      "nodeUris": "{{_elasticUrl}}",
                      "indexFormat": "imoutorebirth-lilin-{0:yyyy.MM}",
                      "restrictedToMinimumLevel": "Information"
                    }
                  }
                ],
                "Properties": {
                  "Application": "ImoutoRebirth.Lilin"
                }
              },
              "Kestrel": {
                "EndPoints": {
                  "Http": {
                    "Url": "http://*:{{_lilinPort}}"
                  }
                }
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
                "MeidoDatabase": "{{_meidoConnectionString}}"
              },
              "RabbitSettings": {
                "Url": "{{_rabbitUrl}}",
                "Username": "{{_rabbitUsername}}",
                "Password": "{{_rabbitPassword}}"
              },
              "Serilog": {
                "WriteTo": [
                  {
                    "Name": "Elasticsearch",
                    "Args": {
                      "nodeUris": "{{_elasticUrl}}",
                      "indexFormat": "imoutorebirth-meido-{0:yyyy.MM}",
                      "restrictedToMinimumLevel": "Information"
                    }
                  }
                ],
                "Properties": {
                  "Application": "ImoutoRebirth.Meido"
                }
              },
              "MetadataActualizerSettings": {
                "RepeatEveryMinutes": {{_meidoMetadataActualizerRepeatEveryMinutes}},
                "ActiveSources": [
                  "Yandere", "Danbooru"
                ] 
              },
              "FaultToleranceSettings": {
                "RepeatEveryMinutes": {{_meidoFaultToleranceRepeatEveryMinutes}},
                "IsEnabled": {{_meidoFaultToleranceIsEnabled}}
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
                "RoomDatabase": "{{_roomConnectionString}}"
              },
              "RabbitSettings": {
                "Url": "{{_rabbitUrl}}",
                "Username": "{{_rabbitUsername}}",
                "Password": "{{_rabbitPassword}}"
              },
              "Serilog": {
                "WriteTo": [
                  {
                    "Name": "Elasticsearch",
                    "Args": {
                      "nodeUris": "{{_elasticUrl}}",
                      "indexFormat": "imoutorebirth-room-{0:yyyy.MM}",
                      "restrictedToMinimumLevel": "Information"
                    }
                  }
                ]
              },
              "ImoutoPicsUploadUrl": "{{_roomImoutoPicsUploadUrl}}"
            }
            """;
    }
}
