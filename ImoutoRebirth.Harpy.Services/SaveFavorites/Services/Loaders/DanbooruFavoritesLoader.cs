using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace ImoutoRebirth.Harpy.Services.SaveFavorites.Services.Loaders
{
    internal class DanbooruFavoritesLoader
    {
        private const string FavoritesUrl =
            "https://danbooru.donmai.us/posts.json?tags=ordfav%3A{0}&login={0}&api_key={1}";

        private readonly BooruConfiguration _booruConfiguration;
        private readonly HttpClient _httpClient;

        public DanbooruFavoritesLoader(HttpClient httpClient, IOptions<DanbooruBooruConfiguration> booruConfiguration)
        {
            _httpClient = httpClient;
            _booruConfiguration = booruConfiguration.Value;
        }

        public async IAsyncEnumerable<Post> GetFavoritesUrls()
        {
            Post[] posts;
            var page = 1;
            var url = string.Format(FavoritesUrl, _booruConfiguration.Login, _booruConfiguration.ApiKey);
            do
            {
                var result = await _httpClient.GetStringAsync(url + $"&page={page}");
                posts = JsonSerializer.Deserialize<Post[]>(result) ?? Array.Empty<Post>();

                foreach (var post in posts)
                {
                    yield return post;
                }

                page++;
            } while (posts.Any());
        }
    }
}
