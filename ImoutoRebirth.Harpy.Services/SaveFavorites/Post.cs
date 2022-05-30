using System.Text.Json.Serialization;

namespace ImoutoRebirth.Harpy.Services.SaveFavorites;

internal class Post
{
    [JsonPropertyName("file_url")]
    public string FileUrl { get; set; } = default!;

    [JsonPropertyName("md5")]
    public string Md5 { get; set; } = default!;

    [JsonIgnore]
    public bool WithoutHash { get; set; } = false;
}
