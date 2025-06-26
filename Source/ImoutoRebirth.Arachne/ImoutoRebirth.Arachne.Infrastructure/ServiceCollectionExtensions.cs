using AspNetCoreInjection.TypedFactories;
using Imouto.BooruParser;
using ImoutoRebirth.Arachne.Core.InfrastructureContracts;
using ImoutoRebirth.Arachne.Infrastructure.Abstract;
using ImoutoRebirth.Arachne.Infrastructure.ExHentai;
using ImoutoRebirth.Arachne.Infrastructure.LoaderFabrics;
using ImoutoRebirth.Arachne.Infrastructure.Models.Settings;
using ImoutoRebirth.Arachne.Infrastructure.Schale;
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
        GelbooruSettings gelbooruSettings,
        SankakuSettings sankakuSettings,
        ExHentaiSettings exHentaiSettings)
    {
        services.AddTransient<IExHentaiMetadataProvider, ExHentaiMetadataProvider>();
        services.AddTransient<ExHentaiMetadataProvider>();

        services.AddTransient<ISearchEngine, ExHentaiSearchEngine>();
        services.AddTransient<ExHentaiSearchEngine>();

        services.AddTransient<ExHentaiAuthConfig>(
            _ => new ExHentaiAuthConfig(
                exHentaiSettings.IpbMemberId,
                exHentaiSettings.IpbPassHash,
                exHentaiSettings.Igneous,
                exHentaiSettings.UserAgent));

        services.AddTransient<ISchaleMetadataProvider, SchaleMetadataProvider>();
        services.AddTransient<SchaleMetadataProvider>();

        services.AddTransient<ISearchEngine, SchaleSearchEngine>();
        services.AddTransient<SchaleSearchEngine>();

        // singleton: contains cache of loaders (ensure delays and such)
        services.AddSingleton<ISearchEngineProvider, SearchEngineProvider>();

        // todo find all ifactory and register them
        services.RegisterTypedFactory<BooruSearchEngine.IFactory>().ForConcreteType<BooruSearchEngine>();

        services.AddTransient<YandereLoaderFabric>();
        services.AddTransient<DanbooruLoaderFabric>();
        services.AddTransient<SankakuLoaderFabric>();
        services.AddTransient<GelbooruLoaderFabric>();
        services.AddTransient<Rule34LoaderFabric>();

        services.AddTransient<IBooruLoaderFabric, YandereLoaderFabric>();
        services.AddTransient<IBooruLoaderFabric, DanbooruLoaderFabric>();
        services.AddTransient<IBooruLoaderFabric, SankakuLoaderFabric>();
        services.AddTransient<IBooruLoaderFabric, GelbooruLoaderFabric>();
        services.AddTransient<IBooruLoaderFabric, Rule34LoaderFabric>();

        services.AddTransient<IAvailabilityProvider, YandereLoaderFabric>();
        services.AddTransient<IAvailabilityProvider, DanbooruLoaderFabric>();
        services.AddTransient<IAvailabilityProvider, SankakuLoaderFabric>();
        services.AddTransient<IAvailabilityProvider, GelbooruLoaderFabric>();
        services.AddTransient<IAvailabilityProvider, Rule34LoaderFabric>();
        services.AddTransient<IAvailabilityProvider, ExHentaiMetadataProvider>();
        services.AddTransient<IAvailabilityProvider, SchaleMetadataProvider>();


        services.AddTransient<DanbooruSettings>(x => danbooruSettings);
        services.AddTransient<GelbooruSettings>(x => gelbooruSettings);
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
