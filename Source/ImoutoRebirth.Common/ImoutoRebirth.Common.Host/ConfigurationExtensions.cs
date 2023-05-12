using Microsoft.Extensions.Configuration;

namespace ImoutoRebirth.Common.Host;

public static class ConfigurationExtensions
{
    public static string GetRequiredConnectionString(
        this IConfiguration configuration,
        string name)
    {
        return configuration.GetConnectionString(name)
               ?? throw new InvalidOperationException(
                   $"Missing connection string '{name}'. " +
                   $"Please specify it under the ConnectionStrings:{name} config section");
    }

    public static T GetRequired<T>(this IConfigurationSection configuration) 
        => configuration.Get<T>()
           ?? throw new InvalidOperationException($"Missing configuration");

    public static T GetRequired<T>(this IConfiguration configuration) 
        => configuration.Get<T>()
           ?? throw new InvalidOperationException($"Missing configuration");
}
