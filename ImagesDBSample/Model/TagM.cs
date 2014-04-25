using System;
using System.Collections.Generic;
using System.Linq;
using ImagesDBLibrary.Database;
using ImagesDBLibrary.Database.Access;
using ImagesDBLibrary.Database.Model;

namespace DBConnection.Model
{
    public class TagM
    {
        #region Constructors
        
        private TagM(Tag tag)
        {
            DbId = tag.Id;
            Name = tag.Name;
            Type = TagTypeM.TagTypes.FirstOrDefault(x => x.DbId == tag.Type);
        }

        #endregion Constructors

        #region Properties

        public int DbId { get; private set; }

        public string Name { get; private set; }

        public TagTypeM Type { get; private set; }
        
        #endregion Properties

        #region Methods

        public void Rename(string newName)
        {
            if (!Tags.Contains(this))
            {
                throw new ArgumentException("Source does not exist.");
            }

            ImagesDB.RenameTag(DbId, newName);

            Name = newName;
        }

        public void ChangeType(TagTypeM type)
        {
            if (!Tags.Contains(this))
            {
                throw new ArgumentException("Source does not exist.");
            }

            ImagesDB.ChangeType(DbId, type.DbId);

            Type = type;
        }

        #endregion Methods

        #region Static members

        static TagM()
        {
            Tags = ImagesDB.GetTags().Select(x => new TagM(x)).ToList();
        }

        public static List<TagM> Tags { get; private set; }

        public static TagM Create(string name, TagTypeM tagType)
        {
            if (Tags.Any(x => x.Name == name && x.Type == tagType))
            {
                throw new ArgumentException("Tag with the same name and type already exists.");
            }

            var newTag = ImagesDB.CreateTag(name, tagType.DbId);

            var tag = new TagM(newTag);
            Tags.Add(tag);

            return tag;
        }

        #endregion Static members
    }
}