using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImagesDBLibrary.Database;
using ImagesDBLibrary.Database.Access;
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

        public void Activate()
        {
            Parallel.ForEach
            (
                Collections.Where(x => x != this),
                x => Parallel.ForEach
                         (
                          x.Sources,
                          y => y.UnloadImages()
                         )
            );

            Parallel.ForEach(Sources, x => x.LoadImages());
        }

        public void AddSource(string path)
        {
            if (Sources.Any(x => x.Path == path))
            {
                throw new ArgumentException("The source is already contained in this collection.");
            }

            var source = SourceM.Sources.FirstOrDefault(x => x.Path == path) ?? SourceM.Create(path);

            ImagesDB.AddSourceToCollection(DbId, source.DbId);

            Sources.Add(source);
        }

        public void RemoveSource(SourceM source)
        {
            if (!Sources.Contains(source))
            {
                throw new ArgumentException("Source already deleted.");
            }

            ImagesDB.RemoveSourceFromCollection(DbId, source.DbId);

            Sources.Remove(source);
        }

        public void Remove()
        {
            if (!Collections.Contains(this))
            {
                throw new ArgumentException("Collection already deleted.");
            }

            ImagesDB.RemoveCollection(DbId);

            Collections.Remove(this);
        }

        public void Rename(string newName)
        {
            if (!Collections.Contains(this))
            {
                throw new ArgumentException("Collection does not exist.");
            }
            if (Collections.Any(x => x.Name == newName))
            {
                throw new ArgumentException("Collection with the same name already exists.");
            }

            ImagesDB.RenameCollection(DbId, newName);

            Name = newName;
        }

        #endregion Methods

        #region Static members

        static CollectionM()
        {
            Collections = ImagesDB.GetCollections().Select(x => new CollectionM(x, ImagesDB.GetSources(x))).ToList();
        }

        public static List<CollectionM> Collections { get; private set; }

        public static CollectionM Create(string name)
        {
            if (Collections.Any(x => x.Name == name))
            {
                throw new ArgumentException("Collection with the same name already exists.");
            }

            var newCollection = ImagesDB.CreateCollection(name);

            var collection = new CollectionM(newCollection, new List<Source>());

            Collections.Add(collection);

            return collection;
        }

        #endregion Static members
    }
}