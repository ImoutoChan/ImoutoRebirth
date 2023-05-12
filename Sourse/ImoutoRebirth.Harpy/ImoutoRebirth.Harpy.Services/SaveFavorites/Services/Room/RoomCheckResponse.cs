using System.Text.Json.Serialization;

namespace ImoutoRebirth.Harpy.Services.SaveFavorites.Services.Room;

internal class RoomCheckResponse
{
    [JsonPropertyName("md5")]
    public string Md5 { get; set; } = default!;
}