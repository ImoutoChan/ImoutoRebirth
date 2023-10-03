using AspNetCoreInjection.TypedFactories;
using Imouto.BooruParser;
using ImoutoRebirth.Arachne.Core.InfrastructureContracts;
using ImoutoRebirth.Arachne.Infrastructure.Abstract;
using ImoutoRebirth.Arachne.Infrastructure.LoaderFabrics;
using ImoutoRebirth.Arachne.Infrastructure.Models.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;

namespace ImoutoRebirth.Arachne.Infrastructure;

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

        services.AddTransient<YandereLoaderFabric>();
        services.AddTransient<DanbooruLoaderFabric>();
        services.AddTransient<SankakuLoaderFabric>();
        services.AddTransient<GelbooruLoaderFabric>();
        services.AddTransient<Rule34LoaderFabric>();

        services.AddTransient<IBooruLoaderFabric>(provider => provider.GetRequiredService<YandereLoaderFabric>());
        services.AddTransient<IBooruLoaderFabric>(provider => provider.GetRequiredService<DanbooruLoaderFabric>());
        services.AddTransient<IBooruLoaderFabric>(provider => provider.GetRequiredService<SankakuLoaderFabric>());
        services.AddTransient<IBooruLoaderFabric>(provider => provider.GetRequiredService<GelbooruLoaderFabric>());
        services.AddTransient<IBooruLoaderFabric>(provider => provider.GetRequiredService<Rule34LoaderFabric>());
            

        services.AddTransient<DanbooruSettings>(x => danbooruSettings);
        services.AddTransient<SankakuSettings>(x => sankakuSettings);
        services.AddTransient<IOptions<Imouto.BooruParser.Implementations.Sankaku.SankakuSettings>>(_ =>
            Options.Create(new Imouto.BooruParser.Implementations.Sankaku.SankakuSettings
            {
                Login = sankakuSettings.Login,
                Password = sankakuSettings.Password,
                PauseBetweenRequestsInMs = sankakuSettings.Delay
            }));
        services.AddTransient<IOptions<Imouto.BooruParser.Implementations.Danbooru.DanbooruSettings>>(_ =>
            Options.Create(new Imouto.BooruParser.Implementations.Danbooru.DanbooruSettings
            {
                Login = danbooruSettings.Login,
                ApiKey = danbooruSettings.ApiKey,
                PauseBetweenRequestsInMs = danbooruSettings.Delay,
                BotUserAgent = danbooruSettings.BotUserAgent
            }));

        services.AddTransient<IBooruPostConverter, BooruPostConverter>();

        services.AddMemoryCache();
        services.AddBooruParsers();

        return services;
    }
}
