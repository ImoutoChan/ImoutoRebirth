using System.Collections.Generic;
using ImagesDBLibrary.Database.Model;

namespace ImagesDBLibrary.Database.Access
{
    internal interface IImageDB
    {
        #region Collections

        IEnumerable<Collection> GetCollections();

        void AddCollection(string name);

        void DeleteCollection(Collection collection);

        void RenameCollection(Collection collection);

        #endregion Collections

        #region Sources

        IEnumerable<Source> GetSources(Collection collection);

        void AddSource(Collection collection, string path);

        void DeleteSource(Collection collection, Source source);

        void UpdateSource(Source source);

        #endregion Sources

        #region Images

        IEnumerable<Image> GetImages(Collection collection);

        #endregion Images

        #region TagTypes

        IEnumerable<TagType> GetTagTypes();

        #endregion TagTypes
    }
}
