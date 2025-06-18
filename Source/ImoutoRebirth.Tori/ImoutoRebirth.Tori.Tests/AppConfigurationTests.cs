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
        var appConfig = AppConfiguration.ReadFromDictionary(configDictionary);

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
        Action act = () => AppConfiguration.ReadFromDictionary(configDictionary);
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
        Action act = () => AppConfiguration.ReadFromDictionary(configDictionary);
        act.Should().Throw<Exception>()
            .WithMessage("*Missed configuration keys*")
            .WithMessage("*DanbooruLogin*")
            .WithMessage("*RoomPort*")
            .WithMessage("*OpenSearchUri*");
    }

    [Fact]
    public void ReadFromDictionary_PathWithBackslashes_ConvertsToDoubleBackslashes()
    {
        // arrange
        var configDictionary = CreateValidConfigurationDictionary();
        configDictionary["HarpySavePath"] = @"C:\Path\With\Backslashes";

        // act
        var appConfig = AppConfiguration.ReadFromDictionary(configDictionary);

        // assert
        appConfig.Harpy.SavePath.Should().Be(@"C:\\Path\\With\\Backslashes");
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
    [InlineData(@"C:\\Simple\\Path", @"C:\Simple\Path")]
    [InlineData(@"Path\\With\\\\Double\\\\Backslashes", @"Path\With\\Double\\Backslashes")]
    [InlineData(@"D:\\Program Files\\App", @"D:\Program Files\App")]
    [InlineData(@"Network\\\\Share", @"Network\\Share")]
    public void WriteToDictionary_WithDifferentBackslashFormats_HandlesBackslashesCorrectly(
        string internalPath,
        string expectedPath)
    {
        // arrange
        var testConfig = CreateValidAppConfiguration(internalPath);

        // act
        var dictionary = testConfig.WriteToDictionary();

        // assert
        dictionary["HarpySavePath"].Should().Be(expectedPath);
    }

    [Fact]
    public void ReadFromFile_WithValidFile_ReturnsValidConfiguration()
    {
        // arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            var configDictionary = CreateValidConfigurationDictionary();
            File.WriteAllText(tempFile, JsonSerializer.Serialize(configDictionary));
            var fileInfo = new FileInfo(tempFile);

            // act
            var appConfig = AppConfiguration.ReadFromFile(fileInfo);

            // assert
            appConfig.Should().NotBeNull();
            appConfig.Api.DanbooruLogin.Should().Be("danbooruUser");
            appConfig.Harpy.SavePath.Should().Be(@"C:\\Data\\Harpy");
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
    public void ReadFromFile_WithNonExistentFile_ThrowsException()
    {
        // arrange
        var nonExistentFile = new FileInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));

        // act and assert
        Action act = () => AppConfiguration.ReadFromFile(nonExistentFile);
        act.Should().Throw<Exception>()
            .WithMessage("Unable to find global configuration file!");
    }

    [Fact]
    public void WriteToFile_WritesValidConfiguration()
    {
        // arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            var appConfig = CreateValidAppConfiguration();
            var fileInfo = new FileInfo(tempFile);

            // act
            appConfig.WriteToFile(fileInfo);

            // assert
            var fileContent = File.ReadAllText(tempFile);
            var deserializedDictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(fileContent);

            deserializedDictionary.Should().NotBeNull();
            deserializedDictionary.Should().HaveCount(28);
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
    public void RoundTrip_ReadWriteRead_MaintainsValues()
    {
        // arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            var originalConfig = CreateValidAppConfiguration();
            var fileInfo = new FileInfo(tempFile);

            // act
            originalConfig.WriteToFile(fileInfo);
            var readConfig = AppConfiguration.ReadFromFile(fileInfo);

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
        var readConfig = AppConfiguration.ReadFromDictionary(dictionary);

        // assert
        readConfig.Should().BeEquivalentTo(originalConfig);
    }

    [Fact]
    public void ReadFromFile_MigratesV1ToV2Format()
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

            File.WriteAllText(tempFile, JsonSerializer.Serialize(configDictionary));
            var fileInfo = new FileInfo(tempFile);

            // act
            var appConfig = AppConfiguration.ReadFromFile(fileInfo);

            // assert
            appConfig.Should().NotBeNull();
            appConfig.Connection.MassTransitConnectionString.Should().Be(
                "Server=localhost;Port=5432;Database=masstransit;User Id=postgres;Password=postgres;");

            // check that the V1 keys were removed from the file
            var updatedFileContent = File.ReadAllText(tempFile);
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
    public void ReadFromFile_MigratesV2ToV3Format()
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

            File.WriteAllText(tempFile, JsonSerializer.Serialize(configDictionary));
            var fileInfo = new FileInfo(tempFile);

            // act
            var appConfig = AppConfiguration.ReadFromFile(fileInfo);

            // assert
            appConfig.Should().NotBeNull();
            appConfig.ExHentai.IpbMemberId.Should().Be("");
            appConfig.ExHentai.IpbPassHash.Should().Be("");
            appConfig.ExHentai.Igneous.Should().Be("");
            appConfig.ExHentai.UserAgent.Should().Be("");

            // check that the V3 keys were added to the file
            var updatedFileContent = File.ReadAllText(tempFile);
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

    private static Dictionary<string, string> CreateValidConfigurationDictionary()
    {
        return new Dictionary<string, string>
        {
            ["DanbooruLogin"] = "danbooruUser",
            ["DanbooruApiKey"] = "danbooruKey123",
            ["SankakuLogin"] = "sankakuUser",
            ["SankakuPassword"] = "sankakuPass123",
            ["YandereLogin"] = "yandereUser",
            ["YandereApiKey"] = "yandereKey123",

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

            ["RoomImoutoPicsUploadUrl"] = "https://example.com/upload",

            ["KekkaiAuthToken"] = "token123",

            ["JaegerHost"] = "localhost",
            ["JaegerPort"] = "6831",

            ["ExHentaiIpbMemberId"] = "memberId",
            ["ExHentaiIpbPassHash"] = "passHash",
            ["ExHentaiIgneous"] = "igneous",
            ["ExHentaiUserAgent"] = "Mozilla/5.0",

            ["OpenSearchUri"] = "http://localhost:9200",
            ["InstallLocation"] = @"C:\Apps\ImoutoRebirth"
        };
    }

    private static AppConfiguration CreateValidAppConfiguration(string? harpyPath = null)
    {
        return new AppConfiguration(
            Api: new AppConfiguration.ApiSettings(
                DanbooruLogin: "danbooruUser",
                DanbooruApiKey: "danbooruKey123",
                SankakuLogin: "sankakuUser",
                SankakuPassword: "sankakuPass123",
                YandereLogin: "yandereUser",
                YandereApiKey: "yandereKey123"),
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
                SavePath: harpyPath ?? @"C:\\Data\\Harpy",
                FavoritesSaveJobRepeatEveryMinutes: "60"),
            Meido: new AppConfiguration.MeidoSettings(
                MetadataActualizerRepeatEveryMinutes: "30",
                FaultToleranceRepeatEveryMinutes: "15",
                FaultToleranceIsEnabled: "true"),
            Room: new AppConfiguration.RoomSettings(
                ImoutoPicsUploadUrl: "https://example.com/upload"),
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
            InstallLocation: @"C:\Apps\ImoutoRebirth");
    }
}
