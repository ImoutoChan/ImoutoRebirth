using System;
using System.IO;

namespace ImoutoRebirth.Room.Core.Models
{
    public class DefaultDestinationDirectory : DestinationFolder
    {
        public DefaultDestinationDirectory()
            : base(
                Guid.Empty, 
                Guid.Empty, 
                false, 
                false, 
                DefaultValues.DestinationFolderEntityFormatErrorSubfolder,
                DefaultValues.DestinationFolderEntityHashErrorSubfolder,
                DefaultValues.DestinationFolderEntityWithoutHashErrorSubfolder)
        {
        }
        public override DirectoryInfo GetDestinationDirectory() => null;
    }
}