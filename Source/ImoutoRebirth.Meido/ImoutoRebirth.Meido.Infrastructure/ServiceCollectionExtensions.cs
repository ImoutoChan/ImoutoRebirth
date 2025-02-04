using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Common.Infrastructure;
using ImoutoRebirth.Meido.Application.Infrastructure;
using ImoutoRebirth.Meido.Infrastructure.MetadataRequest;
using ImoutoRebirth.Meido.Infrastructure.MetadataRequest.Requesters;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Meido.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMeidoInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IEventStorage, EventStorage>();
        
        services.AddTransient<ISourceMetadataRequester, SourceMetadataRequester>();
        services.AddTransient<IMetadataRequester, YandereMetadataRequester>();
        services.AddTransient<IMetadataRequester, DanbooruMetadataRequester>();
        services.AddTransient<IMetadataRequester, SankakuMetadataRequester>();
        services.AddTransient<IMetadataRequester, GelbooruMetadataRequester>();
        services.AddTransient<IMetadataRequester, Rule34MetadataRequester>();
        services.AddTransient<IMetadataRequester, ExHentaiMetadataRequester>();

        services.AddDistributedBus();

        return services;
    }
}
