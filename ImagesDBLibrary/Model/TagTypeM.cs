using System;
using System.Collections.Generic;
using System.Linq;
using ImagesDBLibrary.Database.Access;
using ImagesDBLibrary.Database.Model;

namespace ImagesDBLibrary.Model
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
            if (!TagTypes.Contains(this))
            {
                throw new ArgumentException("TagType does not exist.");
            }

            ImagesDB.RemoveTagType(DbId);

            TagTypes.Remove(this);
        }

        public void Rename(string newName)
        {
            if (!TagTypes.Contains(this))
            {
                throw new ArgumentException("TagType does not exist.");
            }

            ImagesDB.RenameTagType(DbId, newName);

            Name = newName;
        }

        #endregion Methods

        #region Static members

        static TagTypeM()
        {
            TagTypes = ImagesDB.GetTagTypes().Select(x => new TagTypeM(x)).ToList();
        }

        public static List<TagTypeM> TagTypes { get; private set; }

        public static TagTypeM Create(string name)
        {
            if (TagTypes.Any(x => x.Name == name))
            {
                throw new ArgumentException("TagType with the same name already exists.");
            }

            var newTagType = ImagesDB.CreateTagType(name);

            var tagType = new TagTypeM(newTagType);
            TagTypes.Add(tagType);

            return tagType;
        }

        #endregion Static members
    }
}