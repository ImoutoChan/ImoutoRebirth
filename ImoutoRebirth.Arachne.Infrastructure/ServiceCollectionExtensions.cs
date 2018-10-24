using System.Net.Http;
using AspNetCoreInjection.TypedFactories;
using ImoutoRebirth.Arachne.Core.InfrastructureContracts;
using ImoutoRebirth.Arachne.Infrastructure.Abstract;
using ImoutoRebirth.Arachne.Infrastructure.LoaderFabrics;
using ImoutoRebirth.Arachne.Infrastructure.Models.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Arachne.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddArachneInfrastructure(
            this IServiceCollection services, 
            DanbooruSettings danbooruSettings, 
            SankakuSettings sankakuSettings)
        {
            services.AddTransient<ISearchEngineProvider, SearchEngineProvider>();
            services.AddTransient<IBooruLoaderFabric, DanbooruLoaderFabric>();
            services.AddTransient<IBooruLoaderFabric, YandereLoaderFabric>();
            services.AddTransient<IBooruLoaderFabric, SankakuLoaderFabric>();

            // todo
            services.RegisterTypedFactory<BooruSearchEngine.IFactory>().ForConcreteType<BooruSearchEngine>();

            services.AddSingleton<HttpClient>();
            services.AddTransient<DanbooruSettings>(x => danbooruSettings);
            services.AddTransient<SankakuSettings>(x => sankakuSettings);

            services.AddTransient<IBooruPostConverter, BooruPostConverter>();

            return services;
        }
    }
}