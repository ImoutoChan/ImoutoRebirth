namespace ImoutoRebirth.Room.DataAccess.Models
{
    public class CustomDestinationFolder : DestinationFolder
    {
        private readonly DirectoryInfo _directoryInfo;

        protected CustomDestinationFolder(
            Guid id,
            Guid collectionId,
            string path,
            bool shouldCreateSubfoldersByHash,
            bool shouldRenameByHash,
            string formatErrorSubfolder,
            string hashErrorSubfolder,
            string withoutHashErrorSubfolder) 
            : base(
                id, 
                collectionId,
                shouldCreateSubfoldersByHash, 
                shouldRenameByHash, 
                formatErrorSubfolder, 
                hashErrorSubfolder, 
                withoutHashErrorSubfolder)
        {
            _directoryInfo = new DirectoryInfo(path);
        }

        public override DirectoryInfo GetDestinationDirectory() => _directoryInfo;
    }
}