using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;
using ImoutoRebirth.Common;
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

        var downloads = new ConcurrentDictionary<string, Download>(StringComparer.OrdinalIgnoreCase);
        await Task.WhenAll(
            SaveFavorites(_danbooruFavoritesLoader.GetFavoritesUrls()),
            SaveFavorites(_yandereFavoritesLoader.GetFavoritesUrls()));

        ct.ThrowIfCancellationRequested();

        var savedCount = downloads.Values.Count(x => x.Saved);
        if (savedCount > 0)
        {
            _logger.LogInformation("Saved {PostsCount} favorite posts", savedCount);
            return true;
        }

        if (downloads.Values.FirstOrDefault(x => x.Error != null)?.Error is { } error)
            ExceptionDispatchInfo.Throw(error);

        _logger.LogTrace("No new posts were found in danbooru or yandere");
        return false;

        async Task SaveFavorites(IAsyncEnumerable<Post> loader)
        {
            var posts = await GetNewPostsFromBooru(loader, ct);

            foreach (var post in posts.Reverse())
            {
                var download = downloads.GetOrAdd(post.Md5, static _ => new Download());
                await download.Gate.WaitAsync(ct);

                try
                {
                    if (download.Saved)
                        continue;

                    await _postSaver.SavePost(post, _saverOptions.Value.SaveToPath, ct);
                    download.Saved = true;
                }
                catch (Exception e) when (!ct.IsCancellationRequested
                                          && e is HttpRequestException or IOException or UnauthorizedAccessException or OperationCanceledException)
                {
                    download.Error = e;
                    _logger.LogWarning(e, "Unable to save favorite post {Md5}", post.Md5);
                }
                finally
                {
                    download.Gate.Release();
                }
            }
        }
    }

    private class Download
    {
        public SemaphoreSlim Gate { get; } = new(1);

        public bool Saved { get; set; }

        public Exception? Error { get; set; }
    }

    private async Task<IReadOnlyCollection<Post>> GetNewPostsFromBooru(
        IAsyncEnumerable<Post> loader,
        CancellationToken ct)
    {
        var uncheckedPosts = new List<Post>(20);
        var checkedPosts = new List<Post>(20);

        try
        {
            await foreach (var favoritePost in loader.WithCancellation(ct))
            {
                uncheckedPosts.Add(favoritePost);

                if (uncheckedPosts.Count < CheckPostsBatchSize)
                    continue;

                var onlyNew = await _roomSavedChecker.GetOnlyNewPosts(uncheckedPosts, ct);
                if (onlyNew.None())
                    return checkedPosts;

                checkedPosts.AddRange(onlyNew);
                uncheckedPosts.Clear();
            }

            if (uncheckedPosts.Any())
            {
                var onlyNew = await _roomSavedChecker.GetOnlyNewPosts(uncheckedPosts, ct);
                
                if (onlyNew.Any())
                    checkedPosts.AddRange(onlyNew);
            }
        }
        catch (Exception e) when (!ct.IsCancellationRequested)
        {
            _logger.LogWarning(e, "Unable to retrieve favorite posts");
            return Array.Empty<Post>();
        }

        return checkedPosts;
    }
}
