using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations.Model;
using System.Linq;
using ImagesDBLibrary.Database;
using ImagesDBLibrary.Database.Model;

namespace DBConnection.Model
{
    public class TagTypeM
    {
        #region Constructors

        private TagTypeM(TagType dbtype)
        {
            DbId = dbtype.Id;
            Name = dbtype.Name;
        }
        
        #endregion Constructors

        #region Properties
        public string Name { get; private set; }

        public int DbId { get; private set; }
        
        #endregion Properties

        #region Methods

        public void Remove()
        {
            //todo
        }

        public void Rename()
        {
            //todo
        }

        #endregion Methods

        #region Static members

        static TagTypeM()
        {
            TagTypes = ImagesDB.GetTagTypes().Select(x => new TagTypeM(x)).ToList();
        }

        public static List<TagTypeM> TagTypes { get; private set; }

        public static void Add(string name)
        {
            if (TagTypes.Any(x => x.Name == name))
            {
                throw new ArgumentException("TagType with the same name already exists.");
            }

            var newTagType = ImagesDB.AddTagType(name);

            TagTypes.Add(new TagTypeM(newTagType));
        }

        #endregion Static members
    }

    //todo remove/rename
}