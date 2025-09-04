using System.Text.Json;
using System.Text.Json.Serialization;
using Flurl.Http.Configuration;
using Imouto.BooruParser.Implementations.Gelbooru;
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
    private readonly GelbooruApiLoader _gelbooruLoader;
    private readonly bool _loadThroughGelbooru = false;

    public DanbooruFavoritesLoader(
        HttpClient httpClient,
        IOptions<DanbooruBooruConfiguration> danbooruConfiguration,
        IOptions<GelbooruBooruConfiguration> gelbooruConfiguration,
        ILogger<DanbooruFavoritesLoader> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _booruConfiguration = danbooruConfiguration.Value;

        _enabled = !(string.IsNullOrWhiteSpace(_booruConfiguration.Login) ||
                     string.IsNullOrWhiteSpace(_booruConfiguration.BotUserAgent));

        _gelbooruLoader = new(
            new FlurlClientCache(),
            Options.Create(
                new GelbooruSettings
                {
                    UserId = int.TryParse(gelbooruConfiguration.Value.UserId, out var userId) ? userId : 0,
                    ApiKey = gelbooruConfiguration.Value.ApiKey,
                    PauseBetweenRequestsInMs = 0
                }));
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
            posts = DeserializePosts(result).ToArray();

            var notEmptyPosts = posts.Where(x => x is { Md5: not null, FileUrl: not null });

            var gelbooruPosts = GetGelbooruUrls(notEmptyPosts);
            
            await foreach (var post in gelbooruPosts)
                yield return post;

            page++;
        } while (posts.Any());
    }

    private static IEnumerable<Post> DeserializePosts(string result)
    {
        // banned posts are returned with null md5 and file_url
        var optionalPosts = JsonSerializer.Deserialize<OptionalPost[]>(result);
        
        if (optionalPosts == null)
            yield break;
        
        foreach (var optionalPost in optionalPosts)
        {
            if (optionalPost is { Md5: not null, FileUrl: not null })
                yield return new() { FileUrl = optionalPost.FileUrl, Md5 = optionalPost.Md5 };
        }
    }

    private async IAsyncEnumerable<Post> GetGelbooruUrls(IEnumerable<Post> notEmptyPosts)
    {
        if (!_loadThroughGelbooru)
        {
            foreach (var post in notEmptyPosts)
                yield return post;

            yield break;
        }
        
        foreach (var post in notEmptyPosts)
        {
            Imouto.BooruParser.Post? gelbooruPost;
            try
            {
                gelbooruPost = await _gelbooruLoader.GetPostByMd5Async(post.Md5);
            }
            catch
            {
                continue;
            }
            
            if (gelbooruPost is { OriginalUrl: not null })
                yield return new Post() { FileUrl = gelbooruPost.OriginalUrl, Md5 = post.Md5 };
        }
    }
}



file class OptionalPost
{
    [JsonPropertyName("file_url")]
    public string? FileUrl { get; set; }

    [JsonPropertyName("md5")]
    public string? Md5 { get; set; }

    [JsonIgnore]
    public bool WithoutHash { get; set; }
}
