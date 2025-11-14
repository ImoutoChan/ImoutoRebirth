using Microsoft.Extensions.Configuration;

namespace ImoutoRebirth.Common.Host;

public static class ConfigurationExtensions
{
    extension(IConfiguration configuration)
    {
        public string GetRequiredConnectionString(string name)
            => configuration.GetConnectionString(name)
               ?? throw new InvalidOperationException(
                   $"Missing connection string '{name}'. "
                   + $"Please specify it under the ConnectionStrings:{name} config section");

        public T GetRequired<T>()
            => configuration.Get<T>()
               ?? throw new InvalidOperationException("Missing configuration");

        public T GetRequiredValue<T>(string key)
            => configuration.GetValue<T>(key) ?? throw new InvalidOperationException($"Missing configuration '{key}'");
    }

    public static T GetRequired<T>(this IConfigurationSection configuration)
        => configuration.Get<T>()
           ?? throw new InvalidOperationException("Missing configuration");
}
