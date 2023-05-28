using System.Text.Json;
using Microsoft.Extensions.Options;

namespace ImoutoRebirth.Harpy.Services.SaveFavorites.Services.Loaders;

internal class YandereFavoritesLoader
{
    private const string FavoritesUrl =
        "https://yande.re/post.json?tags=vote%3A3%3A{0}+order%3Avote+holds%3Aall&login={0}&api_key={1}";

    private readonly BooruConfiguration _booruConfiguration;
    private readonly HttpClient _httpClient;
    private readonly bool _enabled;

    public YandereFavoritesLoader(HttpClient httpClient, IOptions<YandereBooruConfiguration> booruConfiguration)
    {
        _httpClient = httpClient;
        _booruConfiguration = booruConfiguration.Value;

        _enabled = !string.IsNullOrWhiteSpace(_booruConfiguration.Login);
    }

    public async IAsyncEnumerable<Post> GetFavoritesUrls()
    {
        if (!_enabled)
            yield break;

        Post[] posts;
        var page = 1;
        var url = string.Format(FavoritesUrl, _booruConfiguration.Login, _booruConfiguration.ApiKey);
        do
        {
            var result = await _httpClient.GetStringAsync(url + $"&page={page}");
            posts = JsonSerializer.Deserialize<Post[]>(result) ?? Array.Empty<Post>();
            var notEmptyPosts = posts.Where(x => x is { Md5: not null, FileUrl: not null });
            
            foreach (var post in notEmptyPosts)
            {
                post.WithoutHash = true;
                yield return post;
            }

            page++;
        } while (posts.Any());
    }
}
