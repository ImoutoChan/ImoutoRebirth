using System.Globalization;
using ImoutoRebirth.Arachne.Core.InfrastructureContracts;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Common;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

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
            var searchString = Path.GetFileNameWithoutExtension(image.FileName);

            var metadata = await _metadataProvider.SearchMetadataAsync(searchString);

            if (metadata.None())
                return new SearchError(image, SearchEngineType, "Metadata not found");
            
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
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred while searching in ExHentai");
            return new SearchError(image, SearchEngineType, e.ToString());
        }
    }

    private static IEnumerable<Tag> MapTags(FoundMetadata metadata, string imageMd5, string fileName)
    {
        yield return new Tag("LocalMeta", "BooruPostId", metadata.FileIdFromSource);
        yield return new Tag("LocalMeta", "Md5", imageMd5);
        yield return new Tag("LocalMeta", "Score", metadata.Rating.ToString(CultureInfo.InvariantCulture));
        yield return new Tag("Copyright", metadata.Title);

        if (!string.IsNullOrWhiteSpace(metadata.Publisher))
            yield return new Tag("General", metadata.Publisher);

        foreach (var tag in metadata.Tags)
        {
            if (tag.StartsWith("language:"))
                yield return new Tag("Meta", tag["language:".Length..]);

            else if (tag.StartsWith("parody:"))
                yield return new Tag("Copyright", tag["parody:".Length..]);

            else if (tag.StartsWith("group:"))
                yield return new Tag("Artist", tag["group:".Length..]);

            else if (tag.StartsWith("artist:"))
                yield return new Tag("Artist", tag["artist:".Length..]);

            else if (tag.StartsWith("other:"))
                yield return new Tag("Meta", tag["other:".Length..]);

            else if (tag.StartsWith("mixed:"))
                yield return new Tag("General", tag["mixed:".Length..]);

            else if (tag.StartsWith("character:"))
                yield return new Tag("Character", tag["character:".Length..]);

            else
            {
                yield return new Tag("General", tag);
                if (tag.Contains(':'))
                {
                    var withoutTagGroup = tag.Split(':').Last();
                    yield return new Tag("General", withoutTagGroup);
                }
            }
        }
    }

    public Task<LoadedTagsHistory> LoadChangesForTagsSinceHistoryId(int historyId)
        => Task.FromResult(new LoadedTagsHistory([], historyId));

    public Task<LoadedNotesHistory> LoadChangesForNotesSince(DateTimeOffset lastProcessedNoteUpdateAt)
        => Task.FromResult(new LoadedNotesHistory([], lastProcessedNoteUpdateAt));

    private static FoundMetadata ChooseMetadata(string name, IReadOnlyCollection<FoundMetadata> metadata)
    {
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
                return matchingMetadata.MaxBy(x => x.Rating) ?? metadata.First();
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

