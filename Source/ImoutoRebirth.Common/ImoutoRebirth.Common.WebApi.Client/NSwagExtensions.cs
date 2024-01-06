using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ImoutoRebirth.Common.WebApi.Client;

public static class NSwagExtensions
{
    public static IServiceCollection AddNSwagGeneratedWebApiClient<TClient>(this IServiceCollection services, string baseUri)
        where TClient : class
    {
        services.TryAddTransient<GZipCompressingHandler>();
        services.AddHttpClient<TClient>()
            .ConfigureHttpClient(x => x.BaseAddress = new Uri(baseUri))
            .ConfigurePrimaryHttpMessageHandler(_ => new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            })
            .AddHttpMessageHandler<GZipCompressingHandler>();

        return services;
    }
}
