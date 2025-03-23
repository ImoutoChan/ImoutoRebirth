using ImoutoRebirth.Arachne.Core.InfrastructureContracts;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Common;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using System.Web;

namespace ImoutoRebirth.Arachne.Infrastructure.Schale;

public partial class SchaleSearchEngine : ISearchEngine
{
    private readonly ISchaleMetadataProvider _metadataProvider;
    private readonly ILogger<SchaleSearchEngine> _logger;

    public SchaleSearchEngine(
        ISchaleMetadataProvider metadataProvider,
        ILogger<SchaleSearchEngine> logger)
    {
        _metadataProvider = metadataProvider;
        _logger = logger;
    }

    public SearchEngineType SearchEngineType => SearchEngineType.Schale;

    public async Task<SearchResult> Search(Image image)
    {
        try
        {
            var name = Path.GetFileNameWithoutExtension(image.FileName);
            var metadata = await _metadataProvider.SearchAsync(name);

            if (metadata.Any())
                return ToMetadata(image, metadata);

            name = NameCleanRegex().Replace(name, "");
            metadata = await _metadataProvider.SearchAsync(name);

            if (metadata.Any())
                return ToMetadata(image, metadata);

            return Metadata.NotFound(image, SearchEngineType);

        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred while searching in Schale");
            return new SearchError(image, SearchEngineType, e.ToString());
        }
    }

    private SearchResult ToMetadata(Image image, IReadOnlyCollection<SchaleMetadata> metadata)
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

    private static IEnumerable<Tag> MapTags(SchaleMetadata metadata, string imageMd5, string fileName)
    {
        yield return ToTag("LocalMeta", "BooruPostId", metadata.FileIdFromSource);
        yield return ToTag("LocalMeta", "Md5", imageMd5);
        yield return ToTag("Copyright", metadata.Title.ToLower());

        if (!string.IsNullOrWhiteSpace(metadata.Publisher))
            yield return ToTag("General", metadata.Publisher.ToLower());

        yield return ToTag("LocalMeta", "PostedDateTime", metadata.PostedAt.ToString(Constants.DateTimeFormat));

        foreach (var tag in metadata.Tags)
        {
            if (tag.Type == SchaleType.Language)
                yield return ToTag("Meta", tag.Name);

            else if (tag.Type == SchaleType.Parody)
                yield return ToTag("Copyright", tag.Name);

            else if (tag.Type == SchaleType.Circle)
                yield return ToTag("Artist", tag.Name);

            else if (tag.Type == SchaleType.Artist)
                yield return ToTag("Artist", tag.Name);

            else if (tag.Type == SchaleType.Other)
                yield return ToTag("Meta", tag.Name);

            else if (tag.Type == SchaleType.Mixed)
                yield return ToTag("General", tag.Name);

            else if (tag.Type == SchaleType.Character)
                yield return ToTag("Character", FlipCharacterName(tag.Name));

            else
            {
                yield return ToTag("General", tag.Name);

                if (tag.Type == SchaleType.Male)
                    yield return ToTag("General", $"male:{tag.Name}");

                if (tag.Type == SchaleType.Female)
                    yield return ToTag("General", $"female:{tag.Name}");
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

    private static SchaleMetadata ChooseMetadata(string name, IReadOnlyCollection<SchaleMetadata> metadata)
    {
        var potentialLanguageMatches = NameLanguageRegex().Matches(name);

        var nameLanguages = potentialLanguageMatches
            .Select(x => x.Groups[1].Value.Trim())
            .ToList();

        if (nameLanguages.SafeAny())
        {
            var matchingMetadata = metadata
                .Where(x => x.Languages.Any(y => nameLanguages.Any(z => y.Name.Contains(z))))
                .ToList();

            if (matchingMetadata.Any())
                return matchingMetadata.First();
        }

        var englishMetadata = metadata.FirstOrDefault(x => x.Languages.Select(z => z.Name).ContainsIgnoreCase("english"));

        if (englishMetadata != null)
            return englishMetadata;

        var japaneseMetadata = metadata.FirstOrDefault(x => x.Languages.Select(z => z.Name).ContainsIgnoreCase("japanese"));

        if (japaneseMetadata != null)
            return japaneseMetadata;

        return metadata.First();
    }

    [GeneratedRegex(@"\[([^\]]+)\]")]
    private static partial Regex NameLanguageRegex();

    [GeneratedRegex(@"(?:(\([^)]*?\)|\[[^]]*?\]|\{[^}]*?\})(?=\s*(?:(?:\([^)]*?\)|\[[^]]*?\]|\{[^}]*?\})\s*)*$))", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex NameCleanRegex();
}

