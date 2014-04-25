using System;
using System.Collections.Generic;
using System.Linq;
using ImagesDBLibrary.Database.Access;
using ImagesDBLibrary.Database.Model;

namespace ImagesDBLibrary.Model
{
    public class SourceM
    {
        #region Constructors

        private SourceM(Source sourcedb)
        {
            Path = sourcedb.Path;
            DbId = sourcedb.Id;
            IsLoaded = false;
        }

        #endregion Constructors

        #region Properties

        public string Path { get; private set; }
        
        public int DbId { get; private set; }

        public IEnumerable<ImageM> Images { get; private set; }

        public bool IsLoaded { get; private set; }

        #endregion Properties

        #region Methods

        public void LoadImages()
        {
            Images = ImagesDB.GetImages(DbId).Select(x => new ImageM(x, ImagesDB.GetTagsFromImage(x)));
            IsLoaded = true;
        }

        public void UnloadImages()
        {
            Images = new List<ImageM>();
            IsLoaded = false;
        }

        public void Update()
        {
            ImagesDB.UpdateSource();
            LoadImages();
        }

        public void FullUpdate()
        {
            ImagesDB.UpdateSource(true);
            LoadImages();
        }

        public override string ToString()
        {
            return String.Format("IsLoaded: {0} : Path: {1} : DBId: {2}", IsLoaded, Path, DbId);
        }

        #endregion Methods

        #region Static members

        static SourceM()
        {
            Sources = ImagesDB.GetSources().Select(x => new SourceM(x)).ToList();
        }

        public static List<SourceM> Sources { get; private set; }
        
        public static SourceM Create(string path)
        {
            if (Sources.Any(x => x.Path == path))
            {
                throw new ArgumentException("Source with the same path already exists.");
            }

            var newSource = ImagesDB.CreateSource(path);

            var sourceM = new SourceM(newSource);
            Sources.Add(sourceM);

            return sourceM;
        }

        #endregion Static members
    }
}