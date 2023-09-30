using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ImoutoRebirth.Harpy.Services.SaveFavorites.Services.Loaders;

internal class DanbooruFavoritesLoader
{
    private const string FavoritesUrl =
        "https://danbooru.donmai.us/posts.json?tags=ordfav%3A{0}&login={0}&api_key={1}";

    private readonly BooruConfiguration _booruConfiguration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<DanbooruFavoritesLoader> _logger;
    private readonly bool _enabled;

    public DanbooruFavoritesLoader(
        HttpClient httpClient,
        IOptions<DanbooruBooruConfiguration> booruConfiguration,
        ILogger<DanbooruFavoritesLoader> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _booruConfiguration = booruConfiguration.Value;

        _enabled = !(string.IsNullOrWhiteSpace(_booruConfiguration.Login) ||
                     string.IsNullOrWhiteSpace(_booruConfiguration.BotUserAgent));
    }

    public async IAsyncEnumerable<Post> GetFavoritesUrls()
    {
        if (!_enabled)
        {
            _logger.LogInformation("Danbooru favorites loader disabled");
            yield break;
        }

        Post[] posts;
        var page = 1;
        var url = string.Format(FavoritesUrl, _booruConfiguration.Login, _booruConfiguration.ApiKey);
        do
        {
            var result = await _httpClient.GetStringAsync(url + $"&page={page}");
            posts = JsonSerializer.Deserialize<Post[]>(result) ?? Array.Empty<Post>();

            var notEmptyPosts = posts.Where(x => x is { Md5: not null, FileUrl: not null });
            
            foreach (var post in notEmptyPosts)
                yield return post;

            page++;
        } while (posts.Any());
    }
}
