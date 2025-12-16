using System.Text.Json.Serialization;

namespace ImoutoRebirth.Navigator.Slices.Updates.Models;

public class GitHubRelease
{
    [JsonPropertyName("tag_name")]
    public required string TagName { get; init; }

    [JsonPropertyName("prerelease")]
    public bool Prerelease { get; init; }

    [JsonPropertyName("published_at")]
    public DateTimeOffset PublishedAt { get; init; }

    [JsonPropertyName("assets")]
    public IReadOnlyCollection<GitHubAsset> Assets { get; init; } = [];
}

public class GitHubAsset
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("browser_download_url")]
    public required string BrowserDownloadUrl { get; init; }
}
