using System.Net;
using ImoutoRebirth.Common.WebApi.Client;
using ImoutoRebirth.RoomService.WebApi.Client;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Room.WebApi.Client;

public static class WebApiServiceCollectionExtensions
{
    private static IServiceCollection AddWebApiClient<TClient>(
        this IServiceCollection services,
        string baseUri,
        string? httpClientName = null)
        where TClient : class
    {
        httpClientName ??= typeof(TClient).Name;

        services.AddTransient<GZipCompressingHandler>();
        services.AddHttpClient(httpClientName)
            .ConfigurePrimaryHttpMessageHandler(x => new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            })
            .AddHttpMessageHandler<GZipCompressingHandler>();

        services.AddTransient(
            provider =>
            {
                var clientFactory = provider.GetRequiredService<IHttpClientFactory>();
                var client = clientFactory.CreateClient(httpClientName);
                return ClientLambdaActivator.CreateClient<TClient>(baseUri, client);
            });

        return services;
    }

    public static IServiceCollection AddRoomWebApiClients(this IServiceCollection services, string baseUri)
    {
        return services
            .AddWebApiClient<CollectionFilesClient>(baseUri)
            .AddWebApiClient<CollectionsClient>(baseUri)
            .AddWebApiClient<DestinationFolderClient>(baseUri)
            .AddWebApiClient<SourceFoldersClient>(baseUri);
    }
}
