using ImoutoRebirth.LilinService.WebApi.Client;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Lilin.WebApi.Client;

public static class WebApiServiceCollectionExtensions
{
    public static IServiceCollection AddWebApiClient<TClient>(
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

    public static IServiceCollection AddLilinWebApiClients(this IServiceCollection services, string baseUri)
    {
        return services
            .AddWebApiClient<FilesClient>(baseUri)
            .AddWebApiClient<TagTypesClient>(baseUri)
            .AddWebApiClient<TagsClient>(baseUri);
    }
}
