using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ImoutoRebirth.Common.Host;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder SetWorkingDirectory(this WebApplicationBuilder appBuilder)
    {
        Directory.SetCurrentDirectory(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!);

        return appBuilder;
    }
    
    public static HostApplicationBuilder SetWorkingDirectory(this HostApplicationBuilder appBuilder)
    {
        Directory.SetCurrentDirectory(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!);

        return appBuilder;
    }
    
    public static WebApplicationBuilder UseEnvironmentFromEnvironmentVariable(
        this WebApplicationBuilder builder,
        string servicePrefix)
    {
        var environment = Environment.GetEnvironmentVariable($"{servicePrefix}ENVIRONMENT");
        
        builder.Configuration.AddInMemoryCollection(new[]
        {
            new KeyValuePair<string, string?>(HostDefaults.EnvironmentKey, environment)
        });

        return builder;
    }
    
    public static HostApplicationBuilder UseEnvironmentFromEnvironmentVariable(
        this HostApplicationBuilder builder,
        string servicePrefix)
    {
        var environment = Environment.GetEnvironmentVariable($"{servicePrefix}ENVIRONMENT");
        
        builder.Configuration.AddInMemoryCollection(new[]
        {
            new KeyValuePair<string, string?>(HostDefaults.EnvironmentKey, environment)
        });

        return builder;
    }

    public static WebApplicationBuilder UseConfiguration(this WebApplicationBuilder hostBuilder, string servicePrefix)
    {
        hostBuilder.Configuration.AddEnvironmentVariables(servicePrefix);

        return hostBuilder;
    }

    public static HostApplicationBuilder UseConfiguration(this HostApplicationBuilder hostBuilder, string servicePrefix)
    {
        hostBuilder.Configuration.AddEnvironmentVariables(servicePrefix);

        return hostBuilder;
    }
}
