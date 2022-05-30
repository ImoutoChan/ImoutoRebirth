﻿using ImoutoRebirth.Common.Cqrs;
using ImoutoRebirth.Common.Quartz.Extensions;
using ImoutoRebirth.Harpy.Services.SaveFavorites.Commands;
using ImoutoRebirth.Harpy.Services.SaveFavorites.Quartz;
using ImoutoRebirth.Harpy.Services.SaveFavorites.Services;
using ImoutoRebirth.Harpy.Services.SaveFavorites.Services.Loaders;
using ImoutoRebirth.Harpy.Services.SaveFavorites.Services.Room;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Harpy.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHarpyServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMediatR(typeof(FavoritesSaveCommand));
        services.AddLoggingBehavior();

        services.AddQuartzJob<FavoritesSaveJob, FavoritesSaveJob.Description>();

        services.Configure<FavoritesSaveJobSettings>(
            configuration.GetSection(nameof(FavoritesSaveJobSettings)));

        services.Configure<SaverConfiguration>(configuration.GetSection("Saver"));
        services.Configure<DanbooruBooruConfiguration>(configuration.GetSection("Danbooru"));
        services.Configure<YandereBooruConfiguration>(configuration.GetSection("Yandere"));

        services.AddHttpClient<DanbooruFavoritesLoader>();
        services.AddHttpClient<YandereFavoritesLoader>();
        services.AddHttpClient<RoomSavedChecker>();
        services.AddHttpClient<PostSaver>().ConfigureHttpClient(x => x.Timeout = TimeSpan.FromMinutes(5));

        return services;
    }
}