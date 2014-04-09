using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using ImagesDBLibrary.Database.Model;
using Image = ImagesDBLibrary.Database.Model.Image;
using Tag = ImagesDBLibrary.Database.Model.Tag;

namespace ImagesDBLibrary.Database
{
    public static class ImagesDB
    {
        public static IEnumerable<Image> GetAllImages()
        {
            using (var db = new ImagesDBConnection())
            {
                return db.Images;
            }
        }

        public static IEnumerable<Image> GetImagesByTags(IEnumerable<Tag> tags)
        {
            var result = new List<Image>();
            using (var db = new ImagesDBConnection())
            {
                tags = tags.Select(x => db.Tags.Find(x.Id));

                foreach (var tag in tags)
                {
                    var relatedTagSets = tag.TagSets.Where(x => x.Type == TagSetTypesEnum.UserTypeName || x.Type == TagSetTypesEnum.ActualTypeName);
                    foreach (var relatedTagSet in relatedTagSets)
                    {
                        if (!result.Contains(relatedTagSet.Image))
                        {
                            result.Add(relatedTagSet.Image);
                        }
                    }
                }
                return result;
            }
        }

        public static IEnumerable<Image> GetImagesByTags(Tag tag)
        {
            return GetImagesByTags(new List<Tag> {tag});
        }

        public static Dictionary<Tag, int> GetTagsTopFromImages(IEnumerable<Image> images, int? count = null)
        {
            using (var db = new ImagesDBConnection())
            {
                images = images.Select(x => db.Images.Find(x.Id));

                var enumerableImages = images as IList<Image> ?? images.ToList();
                if (!enumerableImages.Any())
                {
                    throw new ArgumentException("Argument 'images' is empty.");
                }

                var result = enumerableImages
                    .SelectMany(x => x
                                         .ActualTagSets
                                         .Union(x.UserTagSets)
                                         .SelectMany(y => y.Tags))
                    .GroupBy(x => x.Id)
                    .OrderByDescending(x => x.Count())
                    .ToDictionary(x => x.First(), x => x.Count());

                if (count != null)
                {
                    result = result.Take(count.GetValueOrDefault()).ToDictionary(x => x.Key, x => x.Value);
                }

                return result;
            }
        }

        public static IEnumerable<Tag> GetTagsFromImage(Image image)
        {
            return image
                .UserTagSets
                .Concat(image.ActualTagSets)
                .Select(x => x.Tags)
                .Aggregate((res, x) => res.Union(x).ToList());
        }

        public static void AddTagsToImages(IEnumerable<Tag> tags, IEnumerable<Image> images)
        {
            using (var db = new ImagesDBConnection())
            {
                tags = tags
                    .Select(x => x.Id)
                    .Distinct()
                    .Select(x => db.Tags.Find(x))
                    .ToList();

                foreach (var image in images)
                {
                    var userTagSet = db
                        .TagSets
                        .First(x => x.Type == TagSetTypesEnum.UserTypeName && x.FKImage == image.Id);

                    foreach (var tag in tags)
                    {
                        userTagSet
                            .Tags
                            .Add(tag);
                    }
                }
                db.SaveChanges();
            }
        }

        public static void RemoveTagsFromImages(IEnumerable<Tag> tags, IEnumerable<Image> images)
        {
            using (var db = new ImagesDBConnection())
            {
                tags = tags.Select(x => db.Tags.Find(x.Id)).ToList();

                foreach (var image in images)
                {
                    var userTagSet =
                        db.TagSets.FirstOrDefault(x =>
                                                  x.Type == TagSetTypesEnum.UserTypeName
                                                  && x.FKImage == image.Id);
                    if (userTagSet == null)
                    {
                        continue;
                    }

                    foreach (var tag in tags)
                    {
                        userTagSet.Tags.Remove(tag);
                    }
                }

                db.SaveChanges();
            }
        }
    }
}
