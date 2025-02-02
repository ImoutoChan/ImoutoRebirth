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
            var metadata = await _metadataProvider.SearchMetadataAsync(image.FileName);

            if (metadata.None())
                return new SearchError(image, SearchEngineType, "Metadata not found");
            
            var chosenMetadata = ChooseMetadata(image.FileName, metadata);

            return new Metadata(
                image,
                SearchEngineType,
                true,
                MapTags(chosenMetadata),
                [],
                chosenMetadata.GalleryId + "|" + chosenMetadata.GalleryToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred while searching in ExHentai");
            return new SearchError(image, SearchEngineType, e.ToString());
        }
    }

    private IEnumerable<Tag> MapTags(FoundMetadata metadata)
    {
        metadata.Title
    }

    public Task<LoadedTagsHistory> LoadChangesForTagsSinceHistoryId(int historyId)
        => Task.FromResult(new LoadedTagsHistory(Array.Empty<string>(), historyId));

    public Task<LoadedNotesHistory> LoadChangesForNotesSince(DateTimeOffset lastProcessedNoteUpdateAt)
        => Task.FromResult(new LoadedNotesHistory([], lastProcessedNoteUpdateAt));

    private static FoundMetadata ChooseMetadata(string name, IReadOnlyCollection<FoundMetadata> metadata)
    {
        var languageMatch = NameLanguageRegex().Match(name);

        var nameLanguage = languageMatch.Success
            ? languageMatch.Groups[1].Value.Trim()
            : null;

        if (nameLanguage != null)
        {
            var matchingMetadata = metadata
                .Where(x => x.Languages.ContainsIgnoreCase(nameLanguage))
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

    [GeneratedRegex(@"\[([^\]]+)\]$")]
    private static partial Regex NameLanguageRegex();
}

