using ImoutoRebirth.Common.Cqrs;
using ImoutoRebirth.Common.Host;
using ImoutoRebirth.Common.Quartz.Extensions;
using ImoutoRebirth.Harpy.Services.SaveFavorites.Commands;
using ImoutoRebirth.Harpy.Services.SaveFavorites.Quartz;
using ImoutoRebirth.Harpy.Services.SaveFavorites.Services;
using ImoutoRebirth.Harpy.Services.SaveFavorites.Services.Loaders;
using ImoutoRebirth.Harpy.Services.SaveFavorites.Services.Room;
using ImoutoRebirth.Room.WebApi.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Harpy.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHarpyServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var userAgent = configuration.GetSection("Danbooru").GetValue<string>("BotUserAgent");
        
        services.AddDefaultMediatR(x => x.RegisterServicesFromAssemblyContaining<FavoritesSaveCommand>());
        services.AddLoggingBehavior();

        services.AddQuartzJob<FavoritesSaveJob, FavoritesSaveJob.Description>();
        services.AddTransient<AdditionalFavoritesSaveJob>();

        services.Configure<FavoritesSaveJobSettings>(
            configuration.GetSection(nameof(FavoritesSaveJobSettings)));
        
        services.Configure<SaverConfiguration>(configuration.GetSection("Saver"));
        services.Configure<DanbooruBooruConfiguration>(configuration.GetSection("Danbooru"));
        services.Configure<GelbooruBooruConfiguration>(configuration.GetSection("Gelbooru"));
        services.Configure<YandereBooruConfiguration>(configuration.GetSection("Yandere"));

        services.AddHttpClient<DanbooruFavoritesLoader>(x =>
        {
            if (!string.IsNullOrWhiteSpace(userAgent))
                x.DefaultRequestHeaders.Add("User-Agent", userAgent);
        });
        services.AddHttpClient<YandereFavoritesLoader>();
        services.AddHttpClient<PostSaver>().ConfigureHttpClient(x =>
        {
            if (!string.IsNullOrWhiteSpace(userAgent))
                x.DefaultRequestHeaders.Add("User-Agent", userAgent);

            x.Timeout = TimeSpan.FromMinutes(5);
        });
        
        services.AddTransient<RoomSavedChecker>();
        services.AddRoomWebApiClients(configuration.GetRequiredValue<string>("Saver:RoomUrl"));

        return services;
    }
}
