using System;
using System.Collections.Generic;

namespace ImoutoViewer.ImoutoRebirthNavigator.NavigatorArgs
{
    internal class ImoutoViewerArgs
    {
        public Guid CollectionId { get; }

        public IReadOnlyCollection<SearchTagDto> SearchTags { get; }

        public ImoutoViewerArgs(Guid collectionId, IReadOnlyCollection<SearchTagDto> searchTags)
        {
            CollectionId = collectionId;
            SearchTags = searchTags;
        }
    }
}