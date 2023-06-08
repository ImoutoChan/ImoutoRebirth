using ImoutoRebirth.LilinService.WebApi.Client;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Lilin.WebApi.Client;

public static class WebApiServiceCollectionExtensions
{
    private static IServiceCollection AddWebApiClient<TClient>(
        this IServiceCollection services,
        string baseUri)
        where TClient : class
    {
        services.AddHttpClient();
        
        var httpClientName = typeof(TClient).Name;

        services.AddTransient(
            provider =>
            {
                var clientFactory = provider.GetRequiredService<IHttpClientFactory>();
                var client = clientFactory.CreateClient(httpClientName);
                return ClientLambdaActivator.CreateClient<TClient>(baseUri, client);
            });

        return services;
    }

    public static IServiceCollection AddLilinWebApiClients(this IServiceCollection services, string baseUri)
    {
        return services
            .AddWebApiClient<FilesClient>(baseUri)
            .AddWebApiClient<TagsClient>(baseUri);
    }
}
