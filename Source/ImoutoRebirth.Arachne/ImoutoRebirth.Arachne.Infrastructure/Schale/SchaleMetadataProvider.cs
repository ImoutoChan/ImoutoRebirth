using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Flurl.Http;
using Flurl.Http.Configuration;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.Infrastructure.Abstract;
using ImoutoRebirth.Common;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Arachne.Infrastructure.Schale;

public interface ISchaleMetadataProvider
{
    Task<IReadOnlyCollection<SchaleMetadata>> SearchAsync(string galleryName);
}

public record SchaleMetadata(
    string Title,
    IReadOnlyCollection<GalleryTag> Authors,
    string Publisher,
    IReadOnlyCollection<GalleryTag> Tags,
    IReadOnlyCollection<GalleryTag> Languages,
    int GalleryId,
    string GalleryToken,
    DateTimeOffset PostedAt)
{
    public string FileIdFromSource => $"{GalleryId}|{GalleryToken}";
}

public sealed partial class SchaleMetadataProvider : ISchaleMetadataProvider, IAvailabilityProvider, IAvailabilityChecker
{
    private const long SchaleRateLimitPauseBetweenRequests = 1500;

    private readonly IFlurlClientCache _flurlClientCache;

    public SchaleMetadataProvider(IFlurlClientCache flurlClientCache)
        => _flurlClientCache = flurlClientCache;

    private static TimeSpan RateLimit => TimeSpan.FromMilliseconds(SchaleRateLimitPauseBetweenRequests);

    public async Task<bool> IsAvailable(CancellationToken ct)
    {
        var isHostAvailable = false;
        try
        {
            var response = await GetClient().Request("books")
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

    public SearchEngineType ForType => SearchEngineType.Schale;

    public IAvailabilityChecker CreateAvailabilityChecker() => this;

    public async Task<IReadOnlyCollection<SchaleMetadata>> SearchAsync(string galleryName)
    {
        if (string.IsNullOrWhiteSpace(galleryName))
            return [];

        var ids = await GetGalleryIds(galleryName);

        if (ids.None())
            return [];

        var detailsTasks = ids.Select(x => GetDetails(x)).ToList();
        var details = await Task.WhenAll(detailsTasks);

        return CreateMetadata(details);
    }

    private async Task<BookDetailResponse> GetDetails(BooksEntryResponse id)
    {
        return await GetClient().Request("books", "detail", id.Id, id.Key)
            .GetAsync()
            .ReceiveJson<BookDetailResponse>();
    }

    private async Task<IReadOnlyCollection<BooksEntryResponse>> GetGalleryIds(string query)
    {
        query = query.Trim();
        query = Regex.Replace(query, "\\s+", "+");

        var searchUrl = GetClient().Request("books")
            .SetQueryParam("s", query, isEncoded: true);

        var response = await searchUrl
            .GetAsync()
            .ReceiveJson<BooksResponse>();

        return response.Entries?.ToArray() ?? [];
    }

    private IFlurlClient GetClient()
    {
        const string baseUrl = "https://api.schale.network";

        return _flurlClientCache
            .GetOrAdd("schale", baseUrl)
            .BeforeCall(async _ => await Throttler.Get("schale").UseAsync(RateLimit))
            .WithHeader("Origin", "https://niyaniya.moe")
            .WithHeader("Accept", "*/*")
            .WithHeader("Accept-Encoding", "gzip, deflate, br")
            .WithHeader("Cache-Control", "no-cache")
            .WithHeader("Accept-Language", "en-US,en;q=0.9")
            .WithHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3")
            .WithHeader("Referer", "https://niyaniya.moe/")
            .WithHeader("Connection", "keep-alive");
    }

    private static IReadOnlyCollection<SchaleMetadata> CreateMetadata(IReadOnlyCollection<BookDetailResponse> details)
        => details
            .Select(
                x =>
                {
                    var (title, publisher) = ParseTitle(x.Title);

                    return new SchaleMetadata(
                        title,
                        x.Tags.Where(y => y.Namespace is 1 or 2).Select(x => x.ToGalleryTag()).ToArray(),
                        publisher,
                        x.Tags.Select(y => y.ToGalleryTag()).ToArray(),
                        x.Tags.Where(y => y.Namespace == 11).Select(y => y.ToGalleryTag()).ToArray(),
                        x.Id,
                        x.Key,
                        DateTimeOffset.FromUnixTimeMilliseconds(x.CreatedAt)
                    );
                })
            .ToList();

    private static (string Title, string Publisher) ParseTitle(string title)
    {
        var pattern = TitleRegex();

        var match = pattern.Match(title);
        return (
            Title: match.Groups["title"].Value.Trim(),
            Publisher: match.Groups["publisher"].Success ? match.Groups["publisher"].Value : ""
        );
    }

    [GeneratedRegex(@"^\s*(?:\((?<publisher>[^)]*)\))?\s*(?:\[(?<author>[^\]]*)\])?\s*(?<title>[^\[\(]+)")]
    private static partial Regex TitleRegex();
}

public record BooksResponse(
    [property: JsonPropertyName("entries")]
    List<BooksEntryResponse> Entries);

public record BooksEntryResponse(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("language")] int Language,
    [property: JsonPropertyName("pages")] int Pages);

public record BookDetailResponse(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("created_at")] long CreatedAt,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("tags")] List<TagResponse> Tags);

public record TagResponse(
    [property: JsonPropertyName("namespace")] int? Namespace,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("count")] int Count)
{
    public GalleryTag ToGalleryTag() => new((SchaleType)(Namespace ?? 0), Name.ToLowerInvariant());
}

public enum SchaleType
{
    Tag = 0,
    Artist = 1,
    Circle = 2,
    Parody = 3,
    Magazine = 4,
    Character = 5,
    Cosplayer = 6,
    Uploader = 7,
    Male = 8,
    Female = 9,
    Mixed = 10,
    Language = 11,
    Other = 12,
    Reclass = 13,
}


public record GalleryTag(SchaleType Type, string Name);
