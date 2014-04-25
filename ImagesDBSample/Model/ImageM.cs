using System;
using System.Collections.Generic;
using System.Linq;
using ImagesDBLibrary.Database;
using ImagesDBLibrary.Database.Access;
using ImagesDBLibrary.Database.Model;

namespace DBConnection.Model
{
    public class ImageM
    {
        #region Constructors

        public ImageM(Image dbimage, IEnumerable<Tag> tags)
        {
            DbId = dbimage.Id;
            Md5 = dbimage.Md5;
            Size = dbimage.Size;
            Path = dbimage.Path;

            Tags = tags.Select(x => TagM.Tags.FirstOrDefault(y => y.DbId == x.Id)).ToList();
        }

        #endregion Constructors

        #region Properties

        public int DbId { get; private set; }

        public List<TagM> Tags { get; private set; }

        public string Md5 { get; private set; }

        public long Size { get; private set; }

        public string Path { get; private set; }

        #endregion Properties

        #region Methods

        public void AddTag(TagM tag)
        {
            if (Tags.Contains(tag))
            {
                throw new ArgumentException("The tag is already contained in this image.");
            }

            ImagesDB.AddTagToImage(DbId, tag.DbId);

            Tags.Add(tag);
        }

        public void RemoveTag(TagM tag)
        {
            if (!Tags.Contains(tag))
            {
                throw new ArgumentException("Tag already removed.");
            }

            ImagesDB.RemoveTagFromImage(DbId, tag.DbId);

            Tags.Remove(tag);
        }

        #endregion Methods
    }
}
