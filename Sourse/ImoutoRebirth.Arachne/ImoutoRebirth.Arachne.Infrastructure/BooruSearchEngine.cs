using Imouto.BooruParser;
using Imouto.BooruParser.Extensions;
using Imouto.BooruParser.Implementations;
using ImoutoRebirth.Arachne.Core.InfrastructureContracts;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.Infrastructure.Abstract;
using Mackiovello.Maybe;
using Microsoft.Extensions.Logging;
using SearchResult = ImoutoRebirth.Arachne.Core.Models.SearchResult;

namespace ImoutoRebirth.Arachne.Infrastructure;

internal class BooruSearchEngine : ISearchEngine
{
    private readonly IBooruApiLoader _booruLoader;
    private readonly IBooruPostConverter _postConverter;
    private readonly ILogger<BooruSearchEngine> _logger;

    public SearchEngineType SearchEngineType { get; }

    public BooruSearchEngine(
        IBooruApiLoader loader, 
        SearchEngineType searchEngineType,
        IBooruPostConverter postConverter,
        ILogger<BooruSearchEngine> logger)
    {
        _postConverter = postConverter;
        _logger = logger;
        SearchEngineType = searchEngineType;
        _booruLoader = loader;
    }

    public async Task<SearchResult> Search(Image image)
    {
        try
        {
            var post = await FindPost(image.Md5);

            return post.SelectOrElse(
                x => _postConverter.Convert(x, image, SearchEngineType),
                () => Metadata.NotFound(image, SearchEngineType));
        }
        catch (Exception e)
        {
            return new SearchError(image, SearchEngineType, e.ToString());
        }
    }

    public async Task<LoadedTagsHistory> LoadChangesForTagsSinceHistoryId(int historyId)
    {
        try
        {
            var history = await LoadTagHistory(historyId);

            var first = history.FirstOrDefault();
            if (first != null)
            {
                var lastHistoryId = history.MaxBy(x => x.HistoryId)!.HistoryId;
                var postIds = history.Select(x => x.PostId).ToArray();
                var parentPostIds = history
                    .Where(x => x.ParentChanged && x.ParentId != null)
                    .Select(x => x.ParentId!.Value)
                    .ToArray();

                var changedPostIds = postIds.Union(parentPostIds).ToList();

                _logger.LogInformation(
                    "Requested tags history loaded with {PostTagUpdatesCount} for {SearchEngine}",
                    history.Count,
                    SearchEngineType);

                return new LoadedTagsHistory(changedPostIds, lastHistoryId);
            }
                
            _logger.LogWarning("Requested tags history is empty");
            return new LoadedTagsHistory(Array.Empty<int>(), historyId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception occured while loading tags history update");
            throw;
        }
    }

    public async Task<LoadedNotesHistory> LoadChangesForNotesSince(DateTimeOffset lastProcessedNoteUpdateAt)
    {
        try
        {
            var history = await LoadNoteHistory(lastProcessedNoteUpdateAt);

            var first = history.FirstOrDefault();
            if (first != null)
            {
                var lastDate = history.MaxBy(x => x.UpdatedAt)!.UpdatedAt;
                var postIds = history.Select(x => x.PostId).ToArray();

                _logger.LogInformation(
                    "Requested notes history loaded with {PostTagUpdatesCount} for {SearchEngine}",
                    history.Count,
                    SearchEngineType);

                return new LoadedNotesHistory(postIds, lastDate);
            }

            _logger.LogWarning("Requested notes history is empty");
            return new LoadedNotesHistory(Array.Empty<int>(), lastProcessedNoteUpdateAt);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception occured while loading notes history update");
            throw;
        }
    }

    private Task<IReadOnlyCollection<NoteHistoryEntry>> LoadNoteHistory(DateTimeOffset lastProcessedNoteUpdateAt)
        => _booruLoader.GetNoteHistoryToDateTimeAsync(lastProcessedNoteUpdateAt).ToListAsync();

    private Task<IReadOnlyCollection<TagHistoryEntry>> LoadTagHistory(int historyId)
        => historyId == default
            ? _booruLoader.GetTagHistoryFirstPageAsync()
            : _booruLoader.GetTagHistoryFromIdToPresentAsync(historyId).ToListAsync();

    private async Task<Maybe<Post>> FindPost(string md5)
    {
        var post = await _booruLoader.GetPostByMd5Async(md5);
        return post!.ToMaybe();
    }

    public interface IFactory
    {
        BooruSearchEngine Create(
            IBooruApiLoader loader,
            SearchEngineType  searchEngineType);
    }
}
