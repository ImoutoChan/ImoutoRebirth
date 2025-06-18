using System.Text.Json;
using ImoutoRebirth.Tori.Configuration;

namespace ImoutoRebirth.Tori.Services;

public interface IConfigurationBuilder
{
    void WriteProductionConfigurations(string newVersion);

    DirectoryInfo GetInstallLocation();
}

public class ConfigurationBuilder : IConfigurationBuilder
{
    private readonly AppConfiguration _configuration;

    public ConfigurationBuilder(AppConfiguration configuration)
        => _configuration = configuration;

    public ConfigurationBuilder(FileInfo globalConfigurationFile)
        => _configuration = globalConfigurationFile.ReadAppConfiguration();

    public void WriteProductionConfigurations(string newVersion)
    {
        var serviceDirectories = GetInstallLocation().GetDirectories();
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

    public DirectoryInfo GetInstallLocation() => new(_configuration.InstallLocation);

    public AppConfiguration GetCurrentConfiguration() => _configuration;

    private string GetArachneConfiguration(string currentVersion)
    {
        return
            $$"""
            {
              "ConnectionStrings": {
                "Masstransit": "{{_configuration.Connection.MassTransitConnectionString}}"
              },
              "DanbooruSettings": {
                "Login": "{{_configuration.Api.DanbooruLogin}}",
                "ApiKey": "{{_configuration.Api.DanbooruApiKey}}",
                "Delay": "1",
                "BotUserAgent": "Arachne/{{currentVersion}}"
              },
              "SankakuSettings": {
                "Login": "{{_configuration.Api.SankakuLogin}}",
                "Password": "{{_configuration.Api.SankakuPassword}}",
                "Delay": "6000"
              },
              "ExHentaiSettings": {
                "IpbMemberId": "{{_configuration.ExHentai.IpbMemberId}}",
                "IpbPassHash": "{{_configuration.ExHentai.IpbPassHash}}",
                "Igneous": "{{_configuration.ExHentai.Igneous}}",
                "UserAgent": "{{_configuration.ExHentai.UserAgent}}"
              },
              "OpenSearchUri": "{{_configuration.OpenSearchUri}}",
              "Jaeger": {
                "Host": "{{_configuration.Jaeger.Host}}",
                "Port": {{_configuration.Jaeger.Port}}
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
                "ApiKey": "{{_configuration.Api.DanbooruApiKey}}",
                "Login": "{{_configuration.Api.DanbooruLogin}}",
                "BotUserAgent": "Harpy/{{currentVersion}}"
              },
              "Yandere": {
                "ApiKey": "{{_configuration.Api.YandereApiKey}}",
                "Login": "{{_configuration.Api.YandereLogin}}"
              },
              "Saver": {
                "SaveToPath": "{{_configuration.Harpy}}",
                "RoomUrl": "http://localhost:{{_configuration.Ports.RoomPort}}/"
              },
              "FavoritesSaveJobSettings": {
                "RepeatEveryMinutes": {{_configuration.Harpy.FavoritesSaveJobRepeatEveryMinutes}}
              },
              "OpenSearchUri": "{{_configuration.OpenSearchUri}}",
              "Jaeger": {
                "Host": "{{_configuration.Jaeger.Host}}",
                "Port": {{_configuration.Jaeger.Port}}
              }
            }
            """;
    }

    private string GetKekkaiConfiguration()
    {
        return
            $$"""
            {
              "AuthToken": "{{_configuration.Kekkai.AuthToken}}",
              "OpenSearchUri": "{{_configuration.OpenSearchUri}}",
              "Kestrel": {
                "EndPoints": {
                  "Http": {
                    "Url": "http://*:{{_configuration.Ports.KekkaiPort}}"
                  }
                }
              },
              "Jaeger": {
                "Host": "{{_configuration.Jaeger.Host}}",
                "Port": {{_configuration.Jaeger.Port}}
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
                "LilinDatabase": "{{_configuration.Connection.LilinConnectionString}}",
                "Masstransit": "{{_configuration.Connection.MassTransitConnectionString}}"
              },
              "OpenSearchUri": "{{_configuration.OpenSearchUri}}",
              "Kestrel": {
                "EndPoints": {
                  "Http": {
                    "Url": "http://*:{{_configuration.Ports.LilinPort}}"
                  }
                }
              },
              "Jaeger": {
                "Host": "{{_configuration.Jaeger.Host}}",
                "Port": {{_configuration.Jaeger.Port}}
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
                "MeidoDatabase": "{{_configuration.Connection.MeidoConnectionString}}",
                "Masstransit": "{{_configuration.Connection.MassTransitConnectionString}}"
              },
              "OpenSearchUri": "{{_configuration.OpenSearchUri}}",
              "MetadataActualizerSettings": {
                "RepeatEveryMinutes": {{_configuration.Meido.MetadataActualizerRepeatEveryMinutes}},
                "ActiveSources": [
                  "Yandere", "Danbooru"
                ] 
              },
              "FaultToleranceSettings": {
                "RepeatEveryMinutes": {{_configuration.Meido.FaultToleranceRepeatEveryMinutes}},
                "IsEnabled": {{_configuration.Meido.FaultToleranceIsEnabled}}
              },
              "Jaeger": {
                "Host": "{{_configuration.Jaeger.Host}}",
                "Port": {{_configuration.Jaeger.Port}}
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
                    "Url": "http://*:{{_configuration.Ports.RoomPort}}"
                  }
                }
              },
              "ConnectionStrings": {
                "RoomDatabase": "{{_configuration.Connection.RoomConnectionString}}",
                "Masstransit": "{{_configuration.Connection.MassTransitConnectionString}}"
              },
              "OpenSearchUri": "{{_configuration.OpenSearchUri}}",
              "ImoutoPicsUploadUrl": "{{_configuration.Room.ImoutoPicsUploadUrl}}",
              "Jaeger": {
                "Host": "{{_configuration.Jaeger.Host}}",
                "Port": {{_configuration.Jaeger.Port}}
              }
            }
            """;
    }
}
