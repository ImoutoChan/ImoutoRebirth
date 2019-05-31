using System;
using System.Linq;
using System.Threading.Tasks;
using ImoutoRebirth.Common;
using ImoutoRebirth.Meido.Core.ParsingStatus;

namespace ImoutoRebirth.Meido.Core.SourceActualizingState
{
    public class SourceActualizerService : ISourceActualizerService
    {
        private readonly ISourceActualizingStateRepository _sourceActualizingStateRepository;
        private readonly IParsingStatusRepository _parsingStatusRepository;

        public SourceActualizerService(
            ISourceActualizingStateRepository sourceActualizingStateRepository,
            IParsingStatusRepository parsingStatusRepository)
        {
            _sourceActualizingStateRepository = sourceActualizingStateRepository;
            _parsingStatusRepository = parsingStatusRepository;
        }

        public async Task RequestActualization()
        {
            var states = await _sourceActualizingStateRepository.GetAll();

            foreach (var state in states)
            {
                state.RequestActualization();
            }
        }

        public async Task MarkTagsUpdated(
            int sourceId, 
            int[] postIds, 
            int lastHistoryId)
        {
            ArgumentValidator.Requires(postIds.Any, nameof(postIds));

            var source = (MetadataSource)sourceId;
            var currentState = await GetStateForSource(source);

            await RequestUpdatesForExisting(postIds, source);

            currentState.SetLastTagUpdate(lastHistoryId);
        }

        public async Task MarkNotesUpdated(
            int sourceId, 
            int[] postIds, 
            DateTimeOffset lastNoteUpdateDate)
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
            var currentState = states.FirstOrDefault(x => x.Source == source);
            return currentState;
        }

        private async Task RequestUpdatesForExisting(int[] postIds, MetadataSource source)
        {
            var posts = await _parsingStatusRepository.Find(new AllWithPostIdFromListSpecification(postIds, source));

            if (posts.Any())
            {
                foreach (var parsingStatuses in posts)
                {
                    parsingStatuses.RequestMetadataUpdate();
                }
            }
        }
    }
}