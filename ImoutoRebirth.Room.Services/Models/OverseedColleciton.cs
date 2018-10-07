using System;
using System.Collections.Generic;
using ImoutoRebirth.Room.DataAccess.Models;

namespace ImoutoRebirth.Room.Core.Models
{
    public class OverseedColleciton
    {
        public Collection Collection { get; }

        public IReadOnlyCollection<SourceFolder> SourceFolders { get; }

        public DestinationFolder DestinationFolder { get; }

        public HashSet<string> ExistedFiles { get; }

        public OverseedColleciton(
            Collection collection,
            IReadOnlyCollection<SourceFolder> sourceFolders,
            HashSet<string> existedFiles,
            DestinationFolder destinationFolder)
        {
            Collection = collection ?? throw new ArgumentNullException(nameof(collection));
            SourceFolders = sourceFolders ?? throw new ArgumentNullException(nameof(sourceFolders));
            ExistedFiles = existedFiles ?? throw new ArgumentNullException(nameof(sourceFolders));
            DestinationFolder = destinationFolder;
        }
    }
}