using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImagesDBLibrary.Database;
using ImagesDBLibrary.Database.Model;

namespace DBConnection.Model
{
    public class CollectionM
    {
        #region Constructors

        private CollectionM(Collection collectiondb, IEnumerable<Source> sources)
        {
            Name = collectiondb.Name;
            DbId = collectiondb.Id;

            Sources = sources.Select(x => SourceM.Sources.FirstOrDefault(y=>y.DbId == x.Id)).ToList();
        }

        #endregion Constructors

        #region Properties

        public string Name { get; private set; }

        public int DbId { get; private set; }

        public List<SourceM> Sources { get; private set; }

        #endregion Properties

        #region Methods

        public void Prepare()
        {
            Parallel.ForEach(Sources, x => x.LoadImages());
        }

        public void AddSource(string path)
        {
            if (Sources.Any(x => x.Path == path))
            {
                throw new ArgumentException("The source is already contained in this collection.");
            }

            var source = SourceM.Sources.FirstOrDefault(x => x.Path == path);
            if (source == null)
            {
                SourceM.Add(path);
                source = SourceM.Sources.First(x => x.Path == path);
            }

            ImagesDB.AddSourceToCollection(DbId, source.DbId);

            Sources.Add(source);
        }

        public void DeleteSource(SourceM source)
        {
            if (!Sources.Contains(source))
            {
                throw new ArgumentException("Source already deleted.");
            }

            ImagesDB.RemoveSourceFromCollection(DbId, source.DbId);

            Sources.Remove(source);
        }

        #endregion Methods

        #region Static members

        static CollectionM()
        {
            Collections = ImagesDB.GetCollections().Select(x => new CollectionM(x, ImagesDB.GetSources(x))).ToList();
        }

        public static List<CollectionM> Collections { get; private set; }

        public static void Add(string name)
        {
            if (Collections.Any(x => x.Name == name))
            {
                throw new ArgumentException("Collection with the same name already exists.");
            }

            var newCollection = ImagesDB.AddCollection(name);

            Collections.Add(new CollectionM(newCollection, new List<Source>()));
        }

        public static void Remove(CollectionM collection)
        {
            if (!Collections.Contains(collection))
            {
                throw new ArgumentException("Collection already deleted.");
            }

            ImagesDB.RemoveCollection(collection.DbId);

            Collections.Remove(collection);
        }

        public static void Rename(CollectionM collection, string newName)
        {
            if (!Collections.Contains(collection))
            {
                throw new ArgumentException("Collection does not exist.");
            }

            ImagesDB.RenameCollection(collection.DbId, newName);

            collection.Name = newName;
        }

        #endregion Static members
    }
}