using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace ImoutoRebirth.Harpy.Services.SaveFavorites.Services.Room
{
    internal class RoomSavedChecker
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<SaverConfiguration> _saverConfiguration;

        public RoomSavedChecker(HttpClient httpClient, IOptions<SaverConfiguration> saverConfiguration)
        {
            _httpClient = httpClient;
            _saverConfiguration = saverConfiguration;
        }

        public async Task<IReadOnlyCollection<Post>> GetOnlyNewPosts(IReadOnlyCollection<Post> allPosts)
        {
            var request = new RoomCheckRequest
            {
                Md5Hashes = allPosts.Select(x => x.Md5).ToArray()
            };

            var results = await _httpClient.PostAsync(
                Path.Combine(_saverConfiguration.Value.RoomUrl, "api/CollectionFiles"),
                new StringContent(JsonSerializer.Serialize(request), Encoding.Unicode, "application/json"));

            results.EnsureSuccessStatusCode();

            var savedResult = await results.Content.ReadAsStringAsync();

            var existed = (JsonSerializer.Deserialize<RoomCheckResponse[]>(savedResult)
                           ?? Array.Empty<RoomCheckResponse>())
                .Select(x => x.Md5)
                .ToHashSet();

            return allPosts.Where(x => !existed.Contains(x.Md5)).ToArray();
        }
    }
}
