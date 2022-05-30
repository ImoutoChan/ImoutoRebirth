using System;
using System.Linq;
using System.Threading.Tasks;
using ImoutoRebirth.Common;
using ImoutoRebirth.Meido.Core.ParsingStatus;
using NodaTime;

namespace ImoutoRebirth.Meido.Core.SourceActualizingState
{
    public class SourceActualizerService : ISourceActualizerService
    {
        private readonly ISourceActualizingStateRepository _sourceActualizingStateRepository;
        private readonly IParsingStatusRepository _parsingStatusRepository;
        private readonly IClock _clock;

        public SourceActualizerService(
            ISourceActualizingStateRepository sourceActualizingStateRepository,
            IParsingStatusRepository parsingStatusRepository,
            IClock clock)
        {
            _sourceActualizingStateRepository = sourceActualizingStateRepository;
            _parsingStatusRepository = parsingStatusRepository;
            _clock = clock;
        }

        public async Task RequestActualization(MetadataSource[] activeSources)
        {
            var now = _clock.GetCurrentInstant();
            var states = await _sourceActualizingStateRepository.GetAll();

            var activeStates = states.Where(x => activeSources.Contains(x.Source));

            foreach (var state in activeStates)
            {
                state.RequestActualization(now);
            }
        }

        public async Task MarkTagsUpdated(
            int sourceId, 
            int[] postIds, 
            int lastHistoryId)
        {
            ArgumentValidator.Requires(postIds.Any, nameof(postIds));

            var now = _clock.GetCurrentInstant();
            var source = (MetadataSource)sourceId;
            var currentState = await GetStateForSource(source);

            await RequestUpdatesForExisting(postIds, source);

            currentState.SetLastTagUpdate(lastHistoryId, now);
        }

        public async Task MarkNotesUpdated(
            int sourceId, 
            int[] postIds, 
            Instant lastNoteUpdateDate)
        {
            ArgumentValidator.Requires(postIds.Any, nameof(postIds));

            var source = (MetadataSource)sourceId;
            var currentState = await GetStateForSource(source);

            await RequestUpdatesForExisting(postIds, source);

            currentState.SetLastNoteUpdate(lastNoteUpdateDate);
        }

        private async Task<SourceActualizingState> GetStateForSource(MetadataSource source)
        {
            var states = await _sourceActualizingStateRepository.GetAll();
            return states.First(x => x.Source == source);
        }

        private async Task RequestUpdatesForExisting(int[] postIds, MetadataSource source)
        {
            var now = _clock.GetCurrentInstant();
            var posts = await _parsingStatusRepository.Find(new AllWithPostIdFromListSpecification(postIds, source));

            if (posts.Any())
            {
                foreach (var parsingStatus in posts)
                {
                    parsingStatus.RequestMetadataUpdate(now);
                }
            }
        }
    }
}
