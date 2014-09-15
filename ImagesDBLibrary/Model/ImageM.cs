using System;
using System.Collections.Generic;
using System.Linq;
using ImagesDBLibrary.Database.Access;
using ImagesDBLibrary.Database.Model;

namespace ImagesDBLibrary.Model
{
    public class ImageM
    {
        #region Constructors

        public ImageM(Image dbimage, IEnumerable<TagsInImage> tags)
        {
            DbId = dbimage.Id;
            Md5 = dbimage.Md5;
            Size = dbimage.Size;
            Path = dbimage.Path;

            TagInfoList = tags.Select(x => new TagInfoM(x)).ToList();
        }

        #endregion Constructors

        #region Properties

        public int DbId { get; private set; }

        public List<TagInfoM> TagInfoList { get; private set; }

        public string Md5 { get; private set; }

        public long Size { get; private set; }

        public string Path { get; private set; }

        //TEMPORARY
        public List<TagM> Tags { get { return TagInfoList.Select(x => x.Tag).ToList(); } }

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

        public void AddTags(IEnumerable<TagM> tags)
        {
            var tagMs = tags as IList<TagM> ?? tags.ToList();

            ImagesDB.AddTagsToImage(DbId, tagMs.Select(x => x.DbId));
            Tags.AddRange(tagMs);
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

        public override string ToString()
        {
            return String.Format("Name: {0}",
                                 Path.Split(new[]
                                            {
                                                "\\"
                                            },
                                            StringSplitOptions.RemoveEmptyEntries).Last());
        }

        public bool ContainsTags(IEnumerable<TagM> tags)
        {
            return tags.All(x => Tags.Contains(x));
        }

        #endregion Methods
    }

    public static class ImageMExtention
    {
        public static Dictionary<TagM, int> GetTopTags(this IEnumerable<ImageM> images, int? count = null)
        {
            var result = images
                .SelectMany(x => x.Tags)
                .GroupBy(x => x.DbId)
                .OrderByDescending(x => x.Count())
                .ToDictionary(x => x.First(), x => x.Count());

            if (count != null)
            {
                result = result.Take(count.GetValueOrDefault()).ToDictionary(x => x.Key, x => x.Value);
            }

            return result;
        }
    }
}
