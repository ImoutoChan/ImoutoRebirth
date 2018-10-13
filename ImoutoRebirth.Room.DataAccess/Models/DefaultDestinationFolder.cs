using System;
using System.IO;

namespace ImoutoRebirth.Room.DataAccess.Models
{
    public class DefaultDestinationFolder : DestinationFolder
    {
        public DefaultDestinationFolder()
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