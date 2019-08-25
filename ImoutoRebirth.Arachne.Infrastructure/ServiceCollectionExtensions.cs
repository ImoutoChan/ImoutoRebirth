using System;
using System.Net.Http;
using AspNetCoreInjection.TypedFactories;
using ImoutoRebirth.Arachne.Core.InfrastructureContracts;
using ImoutoRebirth.Arachne.Infrastructure.Abstract;
using ImoutoRebirth.Arachne.Infrastructure.LoaderFabrics;
using ImoutoRebirth.Arachne.Infrastructure.Models.Settings;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace ImoutoRebirth.Arachne.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddArachneInfrastructure(
            this IServiceCollection services, 
            DanbooruSettings danbooruSettings, 
            SankakuSettings sankakuSettings)
        {
            // singleton: contains cache of loaders (ensure delays and such)
            services.AddSingleton<ISearchEngineProvider, SearchEngineProvider>();

            // todo find all ifactory and register them
            services.RegisterTypedFactory<BooruSearchEngine.IFactory>().ForConcreteType<BooruSearchEngine>();

            var policy 
                = HttpPolicyExtensions.HandleTransientHttpError()
                                      .WaitAndRetryAsync(2, i => TimeSpan.FromMilliseconds(100 * Math.Pow(10, i)));

            services.AddHttpClient<IBooruLoaderFabric, YandereLoaderFabric>()
                    .AddPolicyHandler(policy);
            services.AddHttpClient<IBooruLoaderFabric, DanbooruLoaderFabric>()
                    .AddPolicyHandler(policy);
            services.AddHttpClient<IBooruLoaderFabric, SankakuLoaderFabric>()
                    .AddPolicyHandler(policy);
            

            services.AddTransient<DanbooruSettings>(x => danbooruSettings);
            services.AddTransient<SankakuSettings>(x => sankakuSettings);

            services.AddTransient<IBooruPostConverter, BooruPostConverter>();

            return services;
        }
    }
}