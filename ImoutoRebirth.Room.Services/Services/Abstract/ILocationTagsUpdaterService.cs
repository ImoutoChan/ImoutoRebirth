namespace ImoutoRebirth.Room.Core.Services.Abstract
{
    public interface ILocationTagsUpdaterService
    {
        /// <summary>
        /// Updates location tags of all files in collections, where:
        ///     1. Destination Folder is null;
        ///     2. Source Folder is marked with ShouldAddTagFromFilename flag.
        /// </summary>
        Task UpdateLocationTags();
    }
}