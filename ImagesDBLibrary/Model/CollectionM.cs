using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImagesDBLibrary.Database.Access;
using ImagesDBLibrary.Database.Model;

namespace ImagesDBLibrary.Model
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

        //public IEnumerable<ImageM> Images
        //{
        //    get
        //    {
        //        return Sources.SelectMany(x => x.Images);
        //    }
        //}

        public bool IsActive { get; set; }

        #endregion Properties

        #region Methods
        
        public void Activate(int count = 200)
        {
            //Parallel.ForEach
            //(
            //    Collections.Where(x => x != this),
            //    x => Parallel.ForEach
            //             (
            //              x.Sources.Where(y => !Sources.Contains(y) && y.IsLoaded),
            //              y => y.UnloadImages()
            //             )
            //);

            //Parallel.ForEach(Sources.Where(x => !x.IsLoaded), x => x.LoadImages());


            
            
            foreach (var collection in CollectionM.Collections)
            {
                collection.IsActive = false;
            }

            IsActive = true;

            OnActivatedCollectionChanged(this);
        }

        public SourceM AddSource(string path)
        {
            if (Sources.Any(x => x.Path == path))
            {
                throw new ArgumentException("The source is already contained in this collection.");
            }

            var source = SourceM.Sources.FirstOrDefault(x => x.Path == path) ?? SourceM.Create(path);

            ImagesDB.AddSourceToCollection(DbId, source.DbId);

            Sources.Add(source);

            return SourceM.Sources.FirstOrDefault(x => x.Path == path);
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

        public override string ToString()
        {
            return String.Format("Name: {0} : Id: {1}", Name, DbId);
        }

        public List<ImageM> GetImages(int take = 200, int skip = 0, List<TagM> withTags = null)
        {
            var result = new List<ImageM>();
            withTags = withTags ?? new List<TagM>();
            foreach (var source in Sources)
            {
                result.AddRange(source.GetImages(ref take, ref skip, withTags.Select(x => x.DbId).ToList()));
                if (take == 0)
                {
                    break;
                }
            }
            return result;
        }

        public int CountImagesWithTags(List<TagM> withTags)
        {
            int result = 0;
            withTags = withTags ?? new List<TagM>();
            foreach (var source in Sources)
            {
                result += source.CountImagesWithTags(withTags.Select(x => x.DbId).ToList());
            }
            return result;
        }

        #endregion Methods

        #region Static members

        #region Static constructor

        static CollectionM()
        {
            Collections = ImagesDB.GetCollections().Select(x => new CollectionM(x, ImagesDB.GetSources(x))).ToList();
        }

        #endregion Static constructor

        #region Static properties

        public static List<CollectionM> Collections { get; private set; }

        public static CollectionM ActivatedCollection 
        { 
            get
            {
                return CollectionM.Collections.FirstOrDefault(x => x.IsActive);
            }
        }

        #endregion Static properties

        #region Static methods

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

        #endregion Static method

        #region Static events
        
        public static event EventHandler ActivatedCollectionChanged;

        private static void OnActivatedCollectionChanged(object sender)
        {
            if (ActivatedCollectionChanged != null)
            {
                ActivatedCollectionChanged(sender, new EventArgs());
            }
        }

        #endregion Static events

        #endregion Static members
    }
}