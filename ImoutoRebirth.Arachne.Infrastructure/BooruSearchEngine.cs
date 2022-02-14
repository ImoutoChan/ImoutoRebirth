using Imouto.BooruParser.Loaders;
using Imouto.BooruParser.Model.Base;
using ImoutoRebirth.Arachne.Core.InfrastructureContracts;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.Infrastructure.Abstract;
using Mackiovello.Maybe;
using Microsoft.Extensions.Logging;
using SearchResult = ImoutoRebirth.Arachne.Core.Models.SearchResult;

namespace ImoutoRebirth.Arachne.Infrastructure
{
    internal class BooruSearchEngine : ISearchEngine
    {
        private readonly IBooruAsyncLoader   _booruLoader;
        private readonly IBooruPostConverter _postConverter;
        private readonly ILogger<BooruSearchEngine> _logger;

        public SearchEngineType SearchEngineType { get; }

        public BooruSearchEngine(
            IBooruAsyncLoader loader, 
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
                    var lastHistoryId = first.UpdateId;
                    var postIds = history.Select(x => x.PostId).ToArray();

                    _logger.LogInformation(
                        "Requested tags history loaded with {PostTagUpdatesCount} for {SearchEngine}.",
                        history.Count,
                        SearchEngineType);

                    return new LoadedTagsHistory(postIds, lastHistoryId);
                }
                
                _logger.LogWarning("Requested tags history is empty.");
                return new LoadedTagsHistory(Array.Empty<int>(), historyId);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception occured while loading tags history update.");
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
                    var lastDate = first.Date;
                    var postIds = history.Select(x => x.PostId).ToArray();

                    _logger.LogInformation(
                        "Requested notes history loaded with {PostTagUpdatesCount} for {SearchEngine}.",
                        history.Count,
                        SearchEngineType);

                    return new LoadedNotesHistory(postIds, lastDate);
                }

                _logger.LogWarning("Requested notes history is empty.");
                return new LoadedNotesHistory(Array.Empty<int>(), lastProcessedNoteUpdateAt);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception occured while loading notes history update.");
                throw;
            }
        }

        private Task<List<NoteUpdateEntry>> LoadNoteHistory(DateTimeOffset lastProcessedNoteUpdateAt)
            => _booruLoader.LoadNotesHistoryAsync(lastProcessedNoteUpdateAt.DateTime);

        private Task<List<PostUpdateEntry>> LoadTagHistory(int historyId)
            => historyId == default
                ? _booruLoader.LoadFirstTagHistoryPageAsync()
                : _booruLoader.LoadTagHistoryFromAsync(historyId);

        private async Task<Maybe<Post>> FindPost(string md5)
        {
            var searchResult = await _booruLoader.LoadSearchResultAsync($"md5:{md5}");

            if (!searchResult.NotEmpty)
                return Maybe<Post>.Nothing;
            
            var foundPost = searchResult.Results.First();
            var post = await _booruLoader.LoadPostAsync(foundPost.Id);

            return post.ToMaybe();
        }

        public interface IFactory
        {
            BooruSearchEngine Create(
                IBooruAsyncLoader loader,
                SearchEngineType  searchEngineType);
        }
    }
}