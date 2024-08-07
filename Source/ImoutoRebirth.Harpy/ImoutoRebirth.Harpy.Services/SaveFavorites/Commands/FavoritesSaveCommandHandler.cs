﻿using ImoutoRebirth.Common;
using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Harpy.Services.SaveFavorites.Services;
using ImoutoRebirth.Harpy.Services.SaveFavorites.Services.Loaders;
using ImoutoRebirth.Harpy.Services.SaveFavorites.Services.Room;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ImoutoRebirth.Harpy.Services.SaveFavorites.Commands;

internal class FavoritesSaveCommandHandler : ICommandHandler<FavoritesSaveCommand, bool>
{
    private const int CheckPostsBatchSize = 20;

    private readonly ILogger<FavoritesSaveCommandHandler> _logger;
    private readonly IOptions<SaverConfiguration> _saverOptions;
    private readonly DanbooruFavoritesLoader _danbooruFavoritesLoader;
    private readonly YandereFavoritesLoader _yandereFavoritesLoader;
    private readonly RoomSavedChecker _roomSavedChecker;
    private readonly PostSaver _postSaver;
    private readonly bool _enabled;

    public FavoritesSaveCommandHandler(
        ILogger<FavoritesSaveCommandHandler> logger,
        IOptions<SaverConfiguration> saverOptions,
        DanbooruFavoritesLoader danbooruFavoritesLoader,
        YandereFavoritesLoader yandereFavoritesLoader,
        RoomSavedChecker roomSavedChecker,
        PostSaver postSaver)
    {
        _logger = logger;
        _saverOptions = saverOptions;
        _danbooruFavoritesLoader = danbooruFavoritesLoader;
        _yandereFavoritesLoader = yandereFavoritesLoader;
        _roomSavedChecker = roomSavedChecker;
        _postSaver = postSaver;

        _enabled = !string.IsNullOrWhiteSpace(_saverOptions.Value.SaveToPath)
                   && !string.IsNullOrWhiteSpace(_saverOptions.Value.RoomUrl);
    }

    public async Task<bool> Handle(FavoritesSaveCommand request, CancellationToken ct)
    {
        if (!_enabled)
        {
            _logger.LogInformation("Loading favorite posts disabled, please check configuration");
            return false;
        }
        
        _logger.LogTrace("Loading favorite posts");

        var checkedDanbooruPosts = await GetNewPostsFromBooru(
            _danbooruFavoritesLoader.GetFavoritesUrls(),
            ct);

        var checkedYanderePosts = await GetNewPostsFromBooru(
            _yandereFavoritesLoader.GetFavoritesUrls(),
            ct);

        var postsToSave = checkedDanbooruPosts.Reverse().Union(checkedYanderePosts.Reverse()).ToList();

        if (!postsToSave.Any())
        {
            _logger.LogTrace("No new posts were found in danbooru or yandere");
            return false;
        }

        _logger.LogInformation(
            "Found new posts: Danbooru {DanbooruPostsCount} Yandere {YanderePostsCount}",
            checkedDanbooruPosts.Count,
            checkedYanderePosts.Count);

        await _postSaver.SavePosts(postsToSave, _saverOptions.Value.SaveToPath);

        _logger.LogInformation("Saved {PostsCount} favorite posts", postsToSave.Count);

        return true;
    }

    private async Task<IReadOnlyCollection<Post>> GetNewPostsFromBooru(
        IAsyncEnumerable<Post> loader,
        CancellationToken cancellationToken)
    {
        var uncheckedPosts = new List<Post>(20);
        var checkedPosts = new List<Post>(20);

        try
        {
            await foreach (var favoritePost in loader.WithCancellation(cancellationToken))
            {
                uncheckedPosts.Add(favoritePost);

                if (uncheckedPosts.Count < CheckPostsBatchSize)
                    continue;

                var onlyNew = await _roomSavedChecker.GetOnlyNewPosts(uncheckedPosts);
                if (onlyNew.None())
                    break;

                checkedPosts.AddRange(onlyNew);
                uncheckedPosts.Clear();
            }

            if (uncheckedPosts.Any())
            {
                var onlyNew = await _roomSavedChecker.GetOnlyNewPosts(uncheckedPosts);
                
                if (onlyNew.Any())
                    checkedPosts.AddRange(onlyNew);
            }
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Unable to retrieve favorite posts");
            return Array.Empty<Post>();
        }

        return checkedPosts;
    }
}
