using System;
using System.Collections.Generic;

namespace ImoutoRebirth.Room.DataAccess.Models
{
    public class OversawCollection
    {
        public Collection Collection { get; }

        public IReadOnlyCollection<SourceFolder> SourceFolders { get; }

        public DestinationFolder DestinationFolder { get; }

        public OversawCollection(
            Collection collection,
            IReadOnlyCollection<SourceFolder> sourceFolders,
            DestinationFolder destinationFolder)
        {
            Collection = collection ?? throw new ArgumentNullException(nameof(collection));
            SourceFolders = sourceFolders ?? throw new ArgumentNullException(nameof(sourceFolders));
            DestinationFolder = destinationFolder ?? throw new ArgumentNullException(nameof(sourceFolders));
        }
    }
}