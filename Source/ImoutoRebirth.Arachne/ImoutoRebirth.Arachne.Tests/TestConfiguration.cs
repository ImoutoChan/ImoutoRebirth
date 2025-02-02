using ImoutoRebirth.Arachne.Infrastructure.ExHentai;
using Microsoft.Extensions.Configuration;

namespace ImoutoRebirth.Arachne.Tests;

public class TestConfiguration
{
    public static IConfiguration Configuration { get; } = Load();

    public static IConfiguration Load()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.local.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        return config;
    }

    public ExHentaiAuthConfig GetExHentaiAuthConfig()
        => Configuration.GetSection("ExHentai").Get<ExHentaiAuthConfig>()!;
}
