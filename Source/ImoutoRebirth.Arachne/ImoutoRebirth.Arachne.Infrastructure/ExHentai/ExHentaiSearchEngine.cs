using System.Globalization;
using ImoutoRebirth.Arachne.Core.InfrastructureContracts;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Common;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using System.Web;

namespace ImoutoRebirth.Arachne.Infrastructure.ExHentai;

public partial class ExHentaiSearchEngine : ISearchEngine
{
    private readonly IExHentaiMetadataProvider _metadataProvider;
    private readonly ILogger<ExHentaiSearchEngine> _logger;

    public ExHentaiSearchEngine(
        IExHentaiMetadataProvider metadataProvider,
        ILogger<ExHentaiSearchEngine> logger)
    {
        _metadataProvider = metadataProvider;
        _logger = logger;
    }

    public SearchEngineType SearchEngineType => SearchEngineType.ExHentai;

    public async Task<SearchResult> Search(Image image)
    {
        try
        {
            var name = Path.GetFileNameWithoutExtension(image.FileName);
            var metadata = await _metadataProvider.DeepSearchMetadataAsync(name);

            return metadata.Any()
                ? ToMetadata(image, metadata)
                : Metadata.NotFound(image, SearchEngineType);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred while searching in ExHentai");
            return new SearchError(image, SearchEngineType, e.ToString());
        }
    }

    private Metadata ToMetadata(Image image, IReadOnlyCollection<FoundMetadata> metadata)
    {
        var chosenMetadata = ChooseMetadata(image.FileName, metadata);

        return new Metadata(
            image,
            SearchEngineType,
            true,
            MapTags(chosenMetadata, image.Md5, image.FileName)
                .DistinctBy(x => $"{x.Type}{x.Name}{x.Value}")
                .ToList(),
            [],
            chosenMetadata.FileIdFromSource);
    }

    private static IEnumerable<Tag> MapTags(FoundMetadata metadata, string imageMd5, string fileName)
    {
        yield return ToTag("LocalMeta", "BooruPostId", metadata.FileIdFromSource);
        yield return ToTag("LocalMeta", "Md5", imageMd5);
        yield return ToTag("LocalMeta", "Score", metadata.Rating.ToString(CultureInfo.InvariantCulture));
        yield return ToTag("Copyright", metadata.Title.ToLower());

        if (!string.IsNullOrWhiteSpace(metadata.Publisher))
            yield return ToTag("General", metadata.Publisher.ToLower());

        if (!string.IsNullOrWhiteSpace(metadata.Category))
            yield return ToTag("General", metadata.Category.ToLower());

        if (!string.IsNullOrWhiteSpace(metadata.UploaderName))
            yield return ToTag("LocalMeta", "PostedByUsername", metadata.UploaderName);

        yield return ToTag("LocalMeta", "FilesCount", metadata.FilesCount.ToString());
        yield return ToTag("LocalMeta", "FileSize", metadata.FileSize.ToString());
        yield return ToTag("LocalMeta", "PostedDateTime", metadata.PostedAt.ToString(Constants.DateTimeFormat));

        foreach (var tag in metadata.Tags)
        {
            if (tag.StartsWith("language:"))
                yield return ToTag("Meta", tag["language:".Length..]);

            else if (tag.StartsWith("parody:"))
                yield return ToTag("Copyright", tag["parody:".Length..]);

            else if (tag.StartsWith("group:"))
                yield return ToTag("Artist", tag["group:".Length..]);

            else if (tag.StartsWith("artist:"))
                yield return ToTag("Artist", tag["artist:".Length..]);

            else if (tag.StartsWith("other:"))
                yield return ToTag("Meta", tag["other:".Length..]);

            else if (tag.StartsWith("mixed:"))
                yield return ToTag("General", tag["mixed:".Length..]);

            else if (tag.StartsWith("character:"))
                yield return ToTag("Character", FlipCharacterName(tag["character:".Length..]));

            else
            {
                yield return ToTag("General", tag);
                if (tag.Contains(':'))
                {
                    var withoutTagGroup = tag.Split(':').Last();
                    yield return ToTag("General", withoutTagGroup);
                }
            }
        }
    }

    private static Tag ToTag(string type, string name, string? value = null)
        => new(type, HttpUtility.HtmlDecode(name), HttpUtility.HtmlDecode(value));

    private static string FlipCharacterName(string name)
    {
        var split = name.Split(' ');
        return split.Length == 2 ? $"{split[1]} {split[0]}" : name;
    }

    public Task<LoadedTagsHistory> LoadChangesForTagsSinceHistoryId(int historyId)
        => Task.FromResult(new LoadedTagsHistory([], historyId));

    public Task<LoadedNotesHistory> LoadChangesForNotesSince(DateTimeOffset lastProcessedNoteUpdateAt)
        => Task.FromResult(new LoadedNotesHistory([], lastProcessedNoteUpdateAt));

    private static FoundMetadata ChooseMetadata(string name, IReadOnlyCollection<FoundMetadata> metadata)
    {
        if (metadata.Count == 1)
            return metadata.First();

        var potentialLanguageMatches = NameLanguageRegex().Matches(name);

        var nameLanguages = potentialLanguageMatches
            .Select(x => x.Groups[1].Value.Trim())
            .ToList();

        if (nameLanguages.SafeAny())
        {
            var matchingMetadata = metadata
                .Where(x => x.Languages.ContainsAnyOfIgnoreCase(nameLanguages))
                .ToList();

            if (matchingMetadata.Any())
                return matchingMetadata.MaxBy(x => x.Rating) ?? matchingMetadata.First();
        }

        var englishMetadata = metadata.FirstOrDefault(x => x.Languages.ContainsIgnoreCase("english"));

        if (englishMetadata != null)
            return englishMetadata;

        var japaneseMetadata = metadata.FirstOrDefault(x => x.Languages.ContainsIgnoreCase("japanese"));

        if (japaneseMetadata != null)
            return japaneseMetadata;

        return metadata.MaxBy(x => x.Rating) ?? metadata.First();
    }

    [GeneratedRegex(@"\[([^\]]+)\]")]
    private static partial Regex NameLanguageRegex();
}

