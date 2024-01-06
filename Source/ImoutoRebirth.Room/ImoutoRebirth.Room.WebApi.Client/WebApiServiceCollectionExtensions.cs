using ImoutoRebirth.Common.WebApi.Client;
using ImoutoRebirth.RoomService.WebApi.Client;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Room.WebApi.Client;

public static class WebApiServiceCollectionExtensions
{
    public static IServiceCollection AddRoomWebApiClients(this IServiceCollection services, string baseUri)
        => services
            .AddNSwagGeneratedWebApiClient<CollectionFilesClient>(baseUri)
            .AddNSwagGeneratedWebApiClient<CollectionsClient>(baseUri);
}
