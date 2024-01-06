using ImoutoRebirth.Common.WebApi.Client;
using ImoutoRebirth.LilinService.WebApi.Client;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Lilin.WebApi.Client;

public static class WebApiServiceCollectionExtensions
{
    public static IServiceCollection AddLilinWebApiClients(this IServiceCollection services, string baseUri)
    {
        return services
            .AddNSwagGeneratedWebApiClient<FilesClient>(baseUri)
            .AddNSwagGeneratedWebApiClient<TagsClient>(baseUri);
    }
}
