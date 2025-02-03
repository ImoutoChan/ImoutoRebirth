using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Flurl;
using Flurl.Http;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.Infrastructure.Abstract;
using ImoutoRebirth.Common;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Arachne.Infrastructure.ExHentai;

public interface IExHentaiMetadataProvider
{
    Task<IReadOnlyCollection<FoundMetadata>> SearchMetadataAsync(string galleryName);
}

public record FoundMetadata(
    string Title,
    IReadOnlyCollection<string> Authors,
    string Publisher,
    IReadOnlyCollection<string> Tags,
    IReadOnlyCollection<string> Languages,
    decimal Rating,
    string ThumbnailUrl,
    int GalleryId,
    string GalleryToken)
{
    public string FileIdFromSource => $"{GalleryId}|{GalleryToken}";
}

public sealed class ExHentaiMetadataProvider : IExHentaiMetadataProvider, IAvailabilityProvider, IAvailabilityChecker
{
    private readonly ExHentaiAuthConfig _authConfig;
    private readonly ILogger<ExHentaiMetadataProvider> _logger;
    private readonly bool _exHentaiMode;

    public ExHentaiMetadataProvider(
        ExHentaiAuthConfig authConfig,
        ILogger<ExHentaiMetadataProvider> logger)
    {
        if (authConfig.IsFilled())
            _exHentaiMode = true;

        _authConfig = authConfig;
        _logger = logger;
    }

    public SearchEngineType ForType => SearchEngineType.ExHentai;

    public IAvailabilityChecker CreateAvailabilityChecker() => this;

    public async Task<bool> IsAvailable(CancellationToken ct)
    {
        var isHostAvailable = false;
        try
        {
            var response = await GetBaseUrl()
                .WithCookies(GetAuthCookies())
                .WithHeader("User-Agent", _authConfig.UserAgent)
                .WithTimeout(TimeSpan.FromSeconds(15))
                .HeadAsync(cancellationToken: ct);

            isHostAvailable = response.StatusCode / 100 == 2;
        }
        catch
        {
            // ignored
        }

        return isHostAvailable;
    }

    public async Task<IReadOnlyCollection<FoundMetadata>> SearchMetadataAsync(string galleryName)
    {
        if (string.IsNullOrWhiteSpace(galleryName))
        {
            _logger.LogWarning("Attempt to search with empty gallery name");
            return [];
        }

        using var scope = _logger.BeginScope(
            new Dictionary<string, object>
            {
                ["GalleryName"] = galleryName,
                ["ApiEndpoint"] = "https://api.e-hentai.org/api.php"
            });

        try
        {
            _logger.LogDebug("Starting metadata search for {GalleryName}", galleryName);

            var ids = await GetGalleryIds(galleryName);

            if (ids.None())
            {
                _logger.LogDebug("No galleries found for query '{Query}'", galleryName);
                return [];
            }

            var response = await "https://api.e-hentai.org/api.php"
                .WithCookies(GetAuthCookies())
                .WithHeader("User-Agent", _authConfig.UserAgent)
                .PostJsonAsync(
                    new
                    {
                        method = "gdata",
                        gidlist = ids,
                        @namespace = 1
                    })
                .ReceiveJson<ExHentaiApiResponse>();

            _logger.LogInformation(
                "Successfully received response with {Count} results",
                response.GalleryMetadata?.Count ?? 0);

            return ProcessApiResponse(response);
        }
        catch (FlurlHttpException ex)
        {
            var statusCode = ex.StatusCode?.ToString() ?? "unknown";
            _logger.LogError(
                ex,
                "API request failed with status {StatusCode}. Url: {Url}",
                statusCode,
                ex.Call.Request.Url);
            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during metadata search");
            return [];
        }
    }

    public async Task<string[][]> GetGalleryIds(string query)
    {
        try
        {
            var searchUrl = GetBaseUrl()
                .SetQueryParams(
                    new
                    {
                        f_search = query,
                        f_apply = "Search",
                        advsearch = 1,
                        f_sname = "on",
                        f_stags = "on"
                    });

            var html = await searchUrl
                .WithCookies(GetAuthCookies())
                .WithHeader("User-Agent", _authConfig.UserAgent)
                .GetStringAsync();

            var ids = new List<string[]>();
            var pattern = new Regex(@"/g/(?<gid>\d+)/(?<token>\w+)/");

            foreach (Match match in pattern.Matches(html))
            {
                if (match.Success && int.TryParse(match.Groups["gid"].Value, out var gid))
                {
                    ids.Add([gid.ToString(), match.Groups["token"].Value]);
                }
            }

            _logger.LogDebug("Found {Count} galleries for query '{Query}'", ids.Count, query);
            return ids.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search gallery IDs for query '{Query}'", query);
            return [];
        }
    }

    private string GetBaseUrl() => _exHentaiMode ? "https://exhentai.org/" : "https://e-hentai.org/";

    private IReadOnlyCollection<FoundMetadata> ProcessApiResponse(ExHentaiApiResponse? response)
    {
        if (response?.GalleryMetadata is null || response.GalleryMetadata.Count == 0)
        {
            _logger.LogDebug("Empty API response received");
            return [];
        }

        var result = new List<FoundMetadata>();

        foreach (var gmetadata in response.GalleryMetadata)
        {
            try
            {
                var (title, author, publisher) = ParseTitle(gmetadata.Title);

                var rating = decimal.TryParse(gmetadata.Rating, out var r) ? r : 0.00m;

                result.Add(
                    new FoundMetadata(
                        title,
                        [author],
                        publisher,
                        gmetadata.Tags,
                        DetectLanguages(gmetadata.Tags),
                        rating,
                        gmetadata.Thumb,
                        gmetadata.Gid,
                        gmetadata.Token
                    ));

                _logger.LogTrace("Processed metadata for gallery {GalleryId}", gmetadata.Gid);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to process metadata for gallery {GalleryId}",
                    gmetadata.Gid);
            }
        }

        _logger.LogDebug("Successfully processed {Count} metadata entries", result.Count);
        return result.AsReadOnly();
    }

    private IReadOnlyDictionary<string, string> GetAuthCookies()
    {
        if (_authConfig.IsFilled())
        {
            return new Dictionary<string, string>
            {
                ["ipb_member_id"] = _authConfig.IpbMemberId,
                ["ipb_pass_hash"] = _authConfig.IpbPassHash,
                ["igneous"] = _authConfig.Igneous
            };
        }

        return ReadOnlyDictionary<string, string>.Empty;
    }

    public static (string Title, string Author, string Publisher) ParseTitle(string title)
    {
        var pattern = new Regex(
            @"^\s*(?:\((?<publisher>[^)]*)\))?" +
            @"\s*(?:\[(?<author>[^\]]*)\])?" +
            @"\s*(?<title>[^\[\(]+)");

        var match = pattern.Match(title);
        return (
            Title: match.Groups["title"].Value.Trim(),
            Author: match.Groups["author"].Success ? match.Groups["author"].Value : "Unknown",
            Publisher: match.Groups["publisher"].Success ? match.Groups["publisher"].Value : "Unknown"
        );
    }

    public static IReadOnlyCollection<string> DetectLanguages(IReadOnlyCollection<string> tags)
    {
        var languages = new HashSet<string>();
        foreach (var tag in tags)
        {
            if (tag.StartsWith("language:", StringComparison.Ordinal))
                languages.Add(tag.Split(':')[1]);
        }

        return languages.Count > 0 ? languages : new[] { "japanese" };
    }
}

public record ExHentaiAuthConfig(
    [property: JsonPropertyName("ipb_member_id")]
    string? IpbMemberId,
    [property: JsonPropertyName("ipb_pass_hash")]
    string? IpbPassHash,
    [property: JsonPropertyName("igneous")]
    string? Igneous,
    [property: JsonPropertyName("user_agent")]
    string? UserAgent)
{
    [MemberNotNullWhen(true, nameof(IpbMemberId), nameof(IpbPassHash), nameof(Igneous), nameof(UserAgent))]
    public bool IsFilled()
        => !string.IsNullOrWhiteSpace(IpbMemberId)
           && !string.IsNullOrWhiteSpace(IpbPassHash)
           && !string.IsNullOrWhiteSpace(Igneous)
           && !string.IsNullOrWhiteSpace(UserAgent);
}

internal class ExHentaiApiResponse
{
    [JsonPropertyName("gmetadata")]
    public IReadOnlyCollection<GalleryMetadata>? GalleryMetadata { get; init; }
}

internal class GalleryMetadata
{
    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("title_jpn")]
    public required string TitleJpn { get; init; }

    [JsonPropertyName("tags")]
    public required IReadOnlyCollection<string> Tags { get; init; }

    [JsonPropertyName("rating")]
    public required string Rating { get; init; }

    [JsonPropertyName("gid")]
    public int Gid { get; init; }

    [JsonPropertyName("token")]
    public required string Token { get; init; }

    [JsonPropertyName("thumb")]
    public required string Thumb { get; init; }

    [JsonPropertyName("category")]
    public required string Category { get; init; }

    [JsonPropertyName("uploader")]
    public required string Uploader { get; init; }

    [JsonPropertyName("posted")]
    public required string Posted { get; init; }

    [JsonPropertyName("filecount")]
    public required string FileCount { get; init; }

    [JsonPropertyName("filesize")]
    public required int FileSize { get; init; }

    [JsonPropertyName("expunged")]
    public required bool IsExpunged { get; init; }

    [JsonPropertyName("torrentcount")]
    public required string TorrentCount { get; init; }

    [JsonPropertyName("torrents")]
    public required IReadOnlyCollection<GalleryTorrent> Torrents { get; init; }
}

internal class GalleryTorrent
{
    [JsonPropertyName("hash")]
    public required string Hash { get; init; }

    [JsonPropertyName("added")]
    public required string Added { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("tsize")]
    public required string TorrentFileSize { get; init; }

    [JsonPropertyName("fsize")]
    public required string FileSize { get; init; }
}

