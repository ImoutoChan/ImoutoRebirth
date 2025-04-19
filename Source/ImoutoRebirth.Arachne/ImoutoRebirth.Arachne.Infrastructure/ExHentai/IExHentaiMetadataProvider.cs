using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.Infrastructure.Abstract;
using ImoutoRebirth.Common;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Arachne.Infrastructure.ExHentai;

public interface IExHentaiMetadataProvider
{
    Task<IReadOnlyCollection<FoundMetadata>> DeepSearchMetadataAsync(string galleryName);
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
    string GalleryToken,
    string Category,
    string UploaderName,
    int FilesCount,
    long FileSize,
    bool IsExpunged,
    int TorrentCount,
    string JapaneseTitle,
    DateTimeOffset PostedAt)
{
    public string FileIdFromSource => $"{GalleryId}|{GalleryToken}";
}

public sealed partial class ExHentaiMetadataProvider : IExHentaiMetadataProvider, IAvailabilityProvider, IAvailabilityChecker
{
    private const long ExHentaiRateLimitPauseBetweenRequests = 1500;

    private readonly IFlurlClientCache _flurlClientCache;
    private readonly ExHentaiAuthConfig _authConfig;
    private readonly ILogger<ExHentaiMetadataProvider> _logger;
    private readonly bool _exHentaiMode;

    public ExHentaiMetadataProvider(
        IFlurlClientCache flurlClientCache,
        ExHentaiAuthConfig authConfig,
        ILogger<ExHentaiMetadataProvider> logger)
    {
        if (authConfig.IsFilled())
            _exHentaiMode = true;

        _flurlClientCache = flurlClientCache;
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
            var response = await Request()
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

    public async Task<IReadOnlyCollection<FoundMetadata>> DeepSearchMetadataAsync(string galleryName)
    {
        var (language, artist, group, decensored, nameWithoutBracketsAtEnd, nameWithoutBrackets, colorized, nameWithoutBracketsAndHyphen)
            = ParseGalleryNameMeta(galleryName);

        var simpleFullNameSearch = await SearchMetadataAsync(galleryName);
        var filteredSimpleFullNameSearch = FilterResults(simpleFullNameSearch);
        if (filteredSimpleFullNameSearch.Any())
            return filteredSimpleFullNameSearch;

        var withoutBracketsAtTheEnd = await SearchMetadataAsync(nameWithoutBracketsAtEnd);
        var filteredWithoutBracketsAtTheEnd = FilterResults(withoutBracketsAtTheEnd);
        if (filteredWithoutBracketsAtTheEnd.Any())
            return filteredWithoutBracketsAtTheEnd;

        var withoutBrackets = await SearchMetadataAsync(nameWithoutBrackets);
        var filteredWithoutBrackets = FilterResults(withoutBrackets);
        if (filteredWithoutBrackets.Any())
            return filteredWithoutBrackets;

        var withoutBracketsAndHyphen = await SearchMetadataAsync(nameWithoutBracketsAndHyphen);
        var filteredWithoutBracketsAndHyphen = FilterResults(withoutBracketsAndHyphen);
        if (filteredWithoutBracketsAndHyphen.Any())
            return filteredWithoutBracketsAndHyphen;

        return [];

        IReadOnlyCollection<FoundMetadata> FilterResults(IReadOnlyCollection<FoundMetadata> results)
        {
            if (results.Count < 2)
                return results;

            var filtered = results;

            filtered = TryFilter(filtered, x => string.IsNullOrWhiteSpace(language) || x.Languages.Contains(language));
            filtered = TryFilter(filtered, x => string.IsNullOrWhiteSpace(artist) || x.Tags.Any(y => artist.ContainsOrContainedIn(y)));
            filtered = TryFilter(filtered, x => string.IsNullOrWhiteSpace(group) || x.Tags.Any(y => group.ContainsOrContainedIn(y)));
            filtered = TryFilter(filtered, x => decensored == true && x.Tags.Any(y => y.Contains("uncensored"))
                                                || decensored != true && x.Tags.None(y => y.Contains("uncensored")));
            filtered = TryFilter(filtered, x => x.Title.ContainsIgnoreCase(nameWithoutBrackets));
            filtered = TryFilter(filtered, x => x.Title.EqualsIgnoreCase(nameWithoutBrackets));

            filtered = TryFilter(filtered, x =>
            {
                var nameWithoutMultipleSpaces = Regex.Replace(nameWithoutBrackets, @"\s+", " ",
                    RegexOptions.Compiled | RegexOptions.NonBacktracking).ToLower();

                var cleanedTitle1 = new string(x.Title.Where(y => !Path.GetInvalidFileNameChars().Contains(y)).ToArray())
                    .ToLower();

                var cleanedTitle2 = Regex.Replace(cleanedTitle1, @"\s+", " ",
                    RegexOptions.Compiled | RegexOptions.NonBacktracking);

                return cleanedTitle2.Contains(nameWithoutMultipleSpaces) ||
                       nameWithoutMultipleSpaces.Contains(cleanedTitle2);
            });

            filtered = TryFilter(filtered, x => colorized && x.Tags.Any(y => y.Contains("full color"))
                                                || !colorized && x.Tags.None(y => y.Contains("full color")));
            filtered = TryFilter(filtered, x => !string.IsNullOrWhiteSpace(language) || x.Languages.Contains("english"));
            filtered = TryFilter(filtered, x => !string.IsNullOrWhiteSpace(language) || x.Languages.Contains("russian"));
            filtered = TryFilter(filtered, x => !string.IsNullOrWhiteSpace(language) || !x.Languages.Contains("translated"));

            return filtered;
        }

        IReadOnlyCollection<FoundMetadata> TryFilter(
            IReadOnlyCollection<FoundMetadata> input, Func<FoundMetadata, bool> predicate)
        {
            if (input.Count < 2)
                return input;

            var filtered = input.Where(predicate).ToList();
            return filtered.None() ? input : filtered;
        }
    }

    private static NameMeta ParseGalleryNameMeta(string galleryName)
    {
        string? language = null;

        if (galleryName.Contains("English"))
            language = "english";

        if (galleryName.Contains("Russian"))
            language = "russian";

        var artist = ArtistExtractionRegex().Match(galleryName).Groups["artist"].Value.ToLower().Trim();
        var group = ArtistExtractionRegex().Match(galleryName).Groups["group"].Value.ToLower().Trim();

        var decensored = galleryName.Contains("decensored", StringComparison.OrdinalIgnoreCase)
                         || galleryName.Contains("uncensored", StringComparison.OrdinalIgnoreCase);

        var galleryNameWithoutBracketsAtTheEnd = BracketedContentAtTheEndRegex().Replace(galleryName, "").Trim();

        var galleryNameWithoutBrackets = AllBracketedContentRegex().Replace(galleryName, "").Trim();
        var galleryNameWithoutBracketsAndHyphen = galleryNameWithoutBrackets.Replace("-", "").Replace("~", "").Trim();

        var colorized = galleryNameWithoutBrackets.Contains("colorized", StringComparison.OrdinalIgnoreCase);

        return new(language, artist, group, decensored, galleryNameWithoutBracketsAtTheEnd, galleryNameWithoutBrackets,
            colorized, galleryNameWithoutBracketsAndHyphen);
    }

    private record NameMeta(
        string? Language,
        string? Artist,
        string? Group,
        bool? Decensored,
        string NameWithoutBracketsAtEnd,
        string NameWithoutBrackets,
        bool Colorized,
        string NameWithoutBracketsAndHyphen);

    private async Task<IReadOnlyCollection<FoundMetadata>> SearchMetadataAsync(string galleryName)
    {
        if (string.IsNullOrWhiteSpace(galleryName))
            return [];

        var ids = await GetGalleryIds(galleryName);

        if (ids.None())
            return [];

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

        return ProcessApiResponse(response);
    }

    public async Task<string[][]> GetGalleryIds(string query)
    {
        var searchUrl = Request()
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

        foreach (Match match in EHentaiIdRegex().Matches(html))
        {
            if (match.Success && int.TryParse(match.Groups["gid"].Value, out var gid))
            {
                ids.Add([gid.ToString(), match.Groups["token"].Value]);
            }
        }

        return ids.ToArray();
    }

    private static TimeSpan RateLimit => TimeSpan.FromMilliseconds(ExHentaiRateLimitPauseBetweenRequests);

    private IFlurlRequest Request()
    {
        var baseUrl = _exHentaiMode ? "https://exhentai.org/" : "https://e-hentai.org/";

        return _flurlClientCache
            .GetOrAdd(
                "e-hentai",
                baseUrl,
                y => y.BeforeCall(async _ => await Throttler.Get("e-hentai").UseAsync(RateLimit)))
            .Request(baseUrl);
    }

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
                        WebUtility.HtmlDecode(title),
                        [WebUtility.HtmlDecode(author)],
                        WebUtility.HtmlDecode(publisher),
                        gmetadata.Tags,
                        DetectLanguages(gmetadata.Tags),
                        rating,
                        gmetadata.Thumb,
                        gmetadata.Gid,
                        gmetadata.Token,
                        gmetadata.Category,
                        gmetadata.Uploader,
                        gmetadata.FileCount.GetIntOrDefault() ?? 0,
                        gmetadata.FileSize,
                        gmetadata.IsExpunged,
                        gmetadata.TorrentCount.GetInt(),
                        gmetadata.TitleJpn,
                        DateTimeOffset.FromUnixTimeSeconds(gmetadata.Posted.GetInt())
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
        var match = MetadataRegex().Match(title);
        return (
            Title: match.Groups["title"].Value.Trim(),
            Author: match.Groups["author"].Success ? match.Groups["author"].Value : "",
            Publisher: match.Groups["publisher"].Success ? match.Groups["publisher"].Value : ""
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

    [GeneratedRegex(
        @"^[^\[]*\[(?<group>[^\]\(\)]+)\s*(\((?<artist>[^\]\(\)]+)\))*\]",
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.NonBacktracking)]
    private static partial Regex ArtistExtractionRegex();

    [GeneratedRegex(
        @"(?:(\([^)]*?\)|\[[^]]*?\]|\{[^}]*?\})(?=\s*(?:(?:\([^)]*?\)|\[[^]]*?\]|\{[^}]*?\})\s*)*$))",
        RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex BracketedContentAtTheEndRegex();

    [GeneratedRegex(
        @"(\[[^\]]*\]|\{[^\}]*\}|\([^\)]*\))",
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.NonBacktracking)]
    private static partial Regex AllBracketedContentRegex();

    [GeneratedRegex(
        @"^\s*(?:\((?<publisher>[^)]*)\))?\s*(?:\[(?<author>[^\]]*)\])?\s*(?<title>[^\[\(]+)",
        RegexOptions.Compiled | RegexOptions.NonBacktracking)]
    private static partial Regex MetadataRegex();
    [GeneratedRegex(@"/g/(?<gid>\d+)/(?<token>\w+)/")]
    private static partial Regex EHentaiIdRegex();
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
    public required long FileSize { get; init; }

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
