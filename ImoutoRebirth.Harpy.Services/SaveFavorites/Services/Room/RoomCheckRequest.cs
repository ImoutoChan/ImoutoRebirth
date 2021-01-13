using System.Text.Json.Serialization;

namespace ImoutoRebirth.Harpy.Services.SaveFavorites.Services.Room
{
    internal class RoomCheckRequest
    {
        [JsonPropertyName("md5")]
        public string[] Md5Hashes { get; set; } = default!;
    }
}
