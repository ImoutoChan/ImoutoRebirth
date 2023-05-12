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
        services.AddHttpClient();
        
        httpClientName = typeof(TClient).Name;

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
