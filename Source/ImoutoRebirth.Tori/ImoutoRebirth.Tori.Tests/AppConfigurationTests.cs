using System.Text.Json;
using AwesomeAssertions;
using ImoutoRebirth.Tori.Configuration;

namespace ImoutoRebirth.Tori.Tests;

public class AppConfigurationTests
{
    [Fact]
    public async Task ReadFromDictionary_WithValidDictionary_ReturnsValidConfiguration()
    {
        // arrange
        var configDictionary = CreateValidConfigurationDictionary();

        // act
        var appConfig = AppConfiguration.ReadFromDictionary(configDictionary, false);

        // assert
        await Verify(appConfig);
    }

    [Fact]
    public void ReadFromDictionary_WithMissingKey_ThrowsException()
    {
        // arrange
        var configDictionary = CreateValidConfigurationDictionary();
        configDictionary.Remove("DanbooruLogin"); // Remove a required key

        // act and assert
        Action act = () => AppConfiguration.ReadFromDictionary(configDictionary, false);
        act.Should().Throw<Exception>()
            .WithMessage("*Missed configuration keys*")
            .WithMessage("*DanbooruLogin*");
    }

    [Fact]
    public void ReadFromDictionary_WithMultipleMissingKeys_ThrowsExceptionWithAllMissingKeys()
    {
        // arrange
        var configDictionary = CreateValidConfigurationDictionary();
        configDictionary.Remove("DanbooruLogin");
        configDictionary.Remove("RoomPort");
        configDictionary.Remove("OpenSearchUri");

        // act and assert
        Action act = () => AppConfiguration.ReadFromDictionary(configDictionary, false);
        act.Should().Throw<Exception>()
            .WithMessage("*Missed configuration keys*")
            .WithMessage("*DanbooruLogin*")
            .WithMessage("*RoomPort*")
            .WithMessage("*OpenSearchUri*");
    }

    [Fact]
    public void ReadFromDictionary_PreservesPathWithBackslashes()
    {
        // arrange
        var configDictionary = CreateValidConfigurationDictionary();
        configDictionary["HarpySavePath"] = @"C:\Path\With\Backslashes";

        // act
        var appConfig = AppConfiguration.ReadFromDictionary(configDictionary, false);

        // assert
        appConfig.Harpy.SavePath.Should().Be(@"C:\Path\With\Backslashes");
    }

    [Fact]
    public async Task WriteToDictionary_WithValidConfiguration_ReturnsValidDictionary()
    {
        // arrange
        var appConfig = CreateValidAppConfiguration();

        // act
        var dictionary = appConfig.WriteToDictionary();

        // assert
        await Verify(dictionary);
    }

    [Theory]
    [InlineData(@"C:\Simple\Path")]
    [InlineData(@"\\server\share")]
    public void WriteToDictionary_PreservesPath(string path)
    {
        var config = CreateValidAppConfiguration(path);

        config.WriteToDictionary()["HarpySavePath"].Should().Be(path);
    }

    [Fact]
    public async Task ProductionConfiguration_RoundTripsPathsAndPasswords()
    {
        var config = CreateValidAppConfiguration();
        config = config with { Api = config.Api with { SankakuPassword = "quoted\"\\value" } };
        var root = Directory.CreateTempSubdirectory();
        try
        {
            var lamia = root.CreateSubdirectory("ImoutoRebirth.Lamia");
            var arachne = root.CreateSubdirectory("ImoutoRebirth.Arachne");

            await new ConfigurationBuilder(config).WriteProductionConfigurations("1.0", root);

            using var media = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(lamia.FullName, "appsettings.Production.json")));
            using var sources = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(arachne.FullName, "appsettings.Production.json")));
            media.RootElement.GetProperty("FFmpeg").GetProperty("Path").GetString().Should().Be(config.FFmpegPath);
            sources.RootElement.GetProperty("SankakuSettings").GetProperty("Password").GetString().Should().Be(config.Api.SankakuPassword);
        }
        finally
        {
            root.Delete(true);
        }
    }

    [Fact]
    public async Task ReadFromFile_WithValidFile_ReturnsValidConfiguration()
    {
        // arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            var configDictionary = CreateValidConfigurationDictionary();
            await File.WriteAllTextAsync(tempFile, JsonSerializer.Serialize(configDictionary));
            var fileInfo = new FileInfo(tempFile);

            // act
            var appConfig = await AppConfiguration.ReadFromFile(fileInfo);

            // assert
            appConfig.Should().NotBeNull();
            appConfig.Api.DanbooruLogin.Should().Be("danbooruUser");
            appConfig.Harpy.SavePath.Should().Be(@"C:\Data\Harpy");
            appConfig.OpenSearchUri.Should().Be("http://localhost:9200");
        }
        finally
        {
            // cleanup
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ReadFromFile_WithNonExistentFile_ThrowsException()
    {
        // arrange
        var nonExistentFile = new FileInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));

        // act and assert
        Func<Task> act = async () => await AppConfiguration.ReadFromFile(nonExistentFile);
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Unable to find global configuration file!");
    }

    [Fact]
    public async Task WriteToFile_WritesValidConfiguration()
    {
        // arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            var appConfig = CreateValidAppConfiguration();
            var fileInfo = new FileInfo(tempFile);

            // act
            await appConfig.WriteToFile(fileInfo);

            // assert
            var fileContent = await File.ReadAllTextAsync(tempFile);
            var deserializedDictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(fileContent);

            deserializedDictionary.Should().NotBeNull();
            deserializedDictionary.Should().HaveCount(33);
            deserializedDictionary["DanbooruLogin"].Should().Be("danbooruUser");
            deserializedDictionary["HarpySavePath"].Should().Be(@"C:\Data\Harpy");
        }
        finally
        {
            // cleanup
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task RoundTrip_ReadWriteRead_MaintainsValues()
    {
        // arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            var originalConfig = CreateValidAppConfiguration();
            var fileInfo = new FileInfo(tempFile);

            // act
            await originalConfig.WriteToFile(fileInfo);
            var readConfig = await AppConfiguration.ReadFromFile(fileInfo);

            // assert
            readConfig.Should().BeEquivalentTo(originalConfig);
        }
        finally
        {
            // cleanup
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void Dictionary_RoundTrip_MaintainsValues()
    {
        // arrange
        var originalConfig = CreateValidAppConfiguration();

        // act
        var dictionary = originalConfig.WriteToDictionary();
        var readConfig = AppConfiguration.ReadFromDictionary(dictionary, false);

        // assert
        readConfig.Should().BeEquivalentTo(originalConfig);
    }

    [Fact]
    public async Task ReadFromFile_MigratesV1ToV2Format()
    {
        // arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            var configDictionary = CreateValidConfigurationDictionary();
            configDictionary.Remove("MassTransitConnectionString");

            // add V1 specific keys
            configDictionary.Add("RabbitMqUrl", "localhost");
            configDictionary.Add("RabbitMqUsername", "guest");
            configDictionary.Add("RabbitMqPassword", "guest");

            await File.WriteAllTextAsync(tempFile, JsonSerializer.Serialize(configDictionary));
            var fileInfo = new FileInfo(tempFile);

            // act
            var appConfig = await AppConfiguration.ReadFromFile(fileInfo);

            // assert
            appConfig.Should().NotBeNull();
            appConfig.Connection.MassTransitConnectionString.Should().Be(
                "Server=localhost;Port=5432;Database=masstransit;User Id=postgres;Password=postgres;");

            // check that the V1 keys were removed from the file
            var updatedFileContent = await File.ReadAllTextAsync(tempFile);
            var updatedDictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(updatedFileContent);

            updatedDictionary.Should().NotBeNull();
            updatedDictionary!.Should().NotContainKey("RabbitMqUrl");
            updatedDictionary.Should().NotContainKey("RabbitMqUsername");
            updatedDictionary.Should().NotContainKey("RabbitMqPassword");
            updatedDictionary.Should().ContainKey("MassTransitConnectionString");
        }
        finally
        {
            // cleanup
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ReadFromFile_MigratesV2ToV3Format()
    {
        // arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            var configDictionary = CreateValidConfigurationDictionary();
            configDictionary.Remove("ExHentaiIpbMemberId");
            configDictionary.Remove("ExHentaiIpbPassHash");
            configDictionary.Remove("ExHentaiIgneous");
            configDictionary.Remove("ExHentaiUserAgent");

            await File.WriteAllTextAsync(tempFile, JsonSerializer.Serialize(configDictionary));
            var fileInfo = new FileInfo(tempFile);

            // act
            var appConfig = await AppConfiguration.ReadFromFile(fileInfo);

            // assert
            appConfig.Should().NotBeNull();
            appConfig.ExHentai.IpbMemberId.Should().Be("");
            appConfig.ExHentai.IpbPassHash.Should().Be("");
            appConfig.ExHentai.Igneous.Should().Be("");
            appConfig.ExHentai.UserAgent.Should().Be("");

            // check that the V3 keys were added to the file
            var updatedFileContent = await File.ReadAllTextAsync(tempFile);
            var updatedDictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(updatedFileContent);

            updatedDictionary.Should().NotBeNull();
            updatedDictionary!.Should().ContainKey("ExHentaiIpbMemberId");
            updatedDictionary.Should().ContainKey("ExHentaiIpbPassHash");
            updatedDictionary.Should().ContainKey("ExHentaiIgneous");
            updatedDictionary.Should().ContainKey("ExHentaiUserAgent");
        }
        finally
        {
            // cleanup
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ReadFromFile_MigratesV3ToV4Format()
    {
        // arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            var configDictionary = CreateValidConfigurationDictionary();
            configDictionary.Remove("FFmpegPath");

            await File.WriteAllTextAsync(tempFile, JsonSerializer.Serialize(configDictionary));
            var fileInfo = new FileInfo(tempFile);

            // act
            var appConfig = await AppConfiguration.ReadFromFile(fileInfo);

            // assert
            appConfig.Should().NotBeNull();
            appConfig.FFmpegPath.Should().Be("");

            // check that the V4 keys were added to the file
            var updatedFileContent = await File.ReadAllTextAsync(tempFile);
            var updatedDictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(updatedFileContent);

            updatedDictionary.Should().NotBeNull();
            updatedDictionary.Should().ContainKey("FFmpegPath");
        }
        finally
        {
            // cleanup
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ReadFromFile_MigratesV6ToV7Format()
    {
        // arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            var configDictionary = CreateValidConfigurationDictionary();
            configDictionary.Remove("Rule34UserId");
            configDictionary.Remove("Rule34ApiKey");

            await File.WriteAllTextAsync(tempFile, JsonSerializer.Serialize(configDictionary));
            var fileInfo = new FileInfo(tempFile);

            // act
            var appConfig = await AppConfiguration.ReadFromFile(fileInfo);

            // assert
            appConfig.Should().NotBeNull();
            appConfig.Api.Rule34ApiKey.Should().Be("");
            appConfig.Api.Rule34UserId.Should().Be("");

            // check that the V7 keys were added to the file
            var updatedFileContent = await File.ReadAllTextAsync(tempFile);
            var updatedDictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(updatedFileContent);

            updatedDictionary.Should().NotBeNull();
            updatedDictionary.Should().ContainKey("Rule34UserId");
            updatedDictionary.Should().ContainKey("Rule34ApiKey");
        }
        finally
        {
            // cleanup
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ReadFromFile_MigratesV7ToV8Format()
    {
        // arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            var configDictionary = CreateValidConfigurationDictionary();
            configDictionary.Remove("DanbooruUserId");

            await File.WriteAllTextAsync(tempFile, JsonSerializer.Serialize(configDictionary));
            var fileInfo = new FileInfo(tempFile);

            // act
            var appConfig = await AppConfiguration.ReadFromFile(fileInfo);

            // assert
            appConfig.Should().NotBeNull();
            appConfig.Api.DanbooruUserId.Should().Be("");

            // check that the V8 key was added to the file
            var updatedFileContent = await File.ReadAllTextAsync(tempFile);
            var updatedDictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(updatedFileContent);

            updatedDictionary.Should().NotBeNull();
            updatedDictionary.Should().ContainKey("DanbooruUserId");
        }
        finally
        {
            // cleanup
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    private static Dictionary<string, string> CreateValidConfigurationDictionary()
    {
        return new Dictionary<string, string>
        {
            ["DanbooruLogin"] = "danbooruUser",
            ["DanbooruApiKey"] = "danbooruKey123",
            ["DanbooruUserId"] = "danbooruUserId",
            ["SankakuLogin"] = "sankakuUser",
            ["SankakuPassword"] = "sankakuPass123",
            ["YandereLogin"] = "yandereUser",
            ["YandereApiKey"] = "yandereKey123",
            ["GelbooruUserId"] = "gelbooruUserId",
            ["GelbooruApiKey"] = "gelbooruKey123",
            ["Rule34UserId"] = "rule34UserId",
            ["Rule34ApiKey"] = "rule34Key123",

            ["LilinConnectionString"] = "Server=localhost;Database=lilin;",
            ["MeidoConnectionString"] = "Server=localhost;Database=meido;",
            ["RoomConnectionString"] = "Server=localhost;Database=room;",
            ["MassTransitConnectionString"] = "Server=localhost;Database=masstransit;",

            ["RoomPort"] = "5000",
            ["KekkaiPort"] = "5001",
            ["LilinPort"] = "5002",

            ["HarpySavePath"] = @"C:\Data\Harpy",
            ["HarpyFavoritesSaveJobRepeatEveryMinutes"] = "60",

            ["MeidoMetadataActualizerRepeatEveryMinutes"] = "30",
            ["MeidoFaultToleranceRepeatEveryMinutes"] = "15",
            ["MeidoFaultToleranceIsEnabled"] = "true",



            ["KekkaiAuthToken"] = "token123",

            ["JaegerHost"] = "localhost",
            ["JaegerPort"] = "6831",

            ["ExHentaiIpbMemberId"] = "memberId",
            ["ExHentaiIpbPassHash"] = "passHash",
            ["ExHentaiIgneous"] = "igneous",
            ["ExHentaiUserAgent"] = "Mozilla/5.0",

            ["OpenSearchUri"] = "http://localhost:9200",
            ["InstallLocation"] = @"C:\Apps\ImoutoRebirth",
            ["FFmpegPath"] = @"C:\Program Files\FFmpeg",
        };
    }

    private static AppConfiguration CreateValidAppConfiguration(string? harpyPath = null)
    {
        return new AppConfiguration(
            Api: new AppConfiguration.ApiSettings(
                DanbooruLogin: "danbooruUser",
                DanbooruApiKey: "danbooruKey123",
                DanbooruUserId: "123456",
                SankakuLogin: "sankakuUser",
                SankakuPassword: "sankakuPass123",
                YandereLogin: "yandereUser",
                YandereApiKey: "yandereKey123",
                GelbooruUserId: "123456",
                GelbooruApiKey: "gelbooruKey123",
                Rule34UserId: "123456",
                Rule34ApiKey: "rule34Key123"),
            Connection: new AppConfiguration.ConnectionSettings(
                LilinConnectionString: "Server=localhost;Database=lilin;",
                MeidoConnectionString: "Server=localhost;Database=meido;",
                RoomConnectionString: "Server=localhost;Database=room;",
                MassTransitConnectionString: "Server=localhost;Database=masstransit;"),
            Ports: new AppConfiguration.PortsSettings(
                RoomPort: "5000",
                KekkaiPort: "5001",
                LilinPort: "5002"),
            Harpy: new AppConfiguration.HarpySettings(
                SavePath: harpyPath ?? @"C:\Data\Harpy",
                FavoritesSaveJobRepeatEveryMinutes: "60"),
            Meido: new AppConfiguration.MeidoSettings(
                MetadataActualizerRepeatEveryMinutes: "30",
                FaultToleranceRepeatEveryMinutes: "15",
                FaultToleranceIsEnabled: "true"),

            Kekkai: new AppConfiguration.KekkaiSettings(
                AuthToken: "token123"),
            Jaeger: new AppConfiguration.JaegerSettings(
                Host: "localhost",
                Port: "6831"),
            ExHentai: new AppConfiguration.ExHentaiSettings(
                IpbMemberId: "memberId",
                IpbPassHash: "passHash",
                Igneous: "igneous",
                UserAgent: "Mozilla/5.0"),
            OpenSearchUri: "http://localhost:9200",
            InstallLocation: @"C:\Apps\ImoutoRebirth",
            FFmpegPath: @"C:\Program Files\FFmpeg",
            WasMigrated: false);
    }
}
