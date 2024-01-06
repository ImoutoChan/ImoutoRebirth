using System.Text.Json.Serialization;

namespace ImoutoRebirth.Harpy.Services.SaveFavorites;

internal class Post
{
    [JsonPropertyName("file_url")]
    public required string FileUrl { get; set; }

    [JsonPropertyName("md5")]
    public required string Md5 { get; set; }

    [JsonIgnore]
    public bool WithoutHash { get; set; }
}
