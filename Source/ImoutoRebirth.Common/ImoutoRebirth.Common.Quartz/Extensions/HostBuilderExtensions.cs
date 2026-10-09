using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ImoutoRebirth.Common.Quartz.Extensions;

public static class HostBuilderExtensions
{
    public static HostApplicationBuilder UseQuartz(
        this HostApplicationBuilder hostBuilder, 
        Action<IServiceCollection>? jobsBuilder = null)
    {
        hostBuilder.Services.AddQuartz();
        jobsBuilder?.Invoke(hostBuilder.Services);

        return hostBuilder;
    }
    
    public static IServiceCollection AddQuartz(
        this IServiceCollection services, 
        Action<IServiceCollection>? jobsBuilder = null)
    {
        services.AddQuartz();
        jobsBuilder?.Invoke(services);
        return services;
    }
}
