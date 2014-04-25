using System;
using System.Collections.Generic;
using System.Linq;
using ImagesDBLibrary.Database.Model;

namespace ImagesDBLibrary.Database.Access
{
    public static partial class ImagesDB
    {
        #region OLD

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
                    var relatedTagSets =
                        tag.TagSets.Where(
                                          x =>
                                          x.Type == TagSetTypesEnum.UserTypeName ||
                                          x.Type == TagSetTypesEnum.ActualTypeName);
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

        public static IEnumerable<Tag> GetTagsStartFrom(string newSearchString, int i)
        {
            try
            {


                using (var db = new ImagesDBConnection())
                {
                    int length = newSearchString.Count();
                    var okTags =
                        db.Tags.Where(x => x.Name.Substring(0, length) == newSearchString)
                          .Take(i)
                          .ToList();

                    if (okTags.Count() < i)
                    {
                        int moar = i - okTags.Count();
                        okTags.AddRange(
                                        db.Tags.Where(x => x.Name.Contains(newSearchString)).Take(moar)
                            );
                    }

                    return okTags.Distinct();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static IEnumerable<Image> GetImages(Collection collection)
        {
            using (var db = new ImagesDBConnection())
            {
                // TODO STORED PROCEDURES ++ GET IMAGES BY COLLECTION ID
                return db.Collections.Find(collection.Id).Sources.SelectMany(x => x.Images);
            }
        }

        public static IEnumerable<Image> Filter(IEnumerable<Image> images, IEnumerable<Tag> tags)
        {
            var result = new List<Image>();
            using (var db = new ImagesDBConnection())
            {
                tags = tags.Select(x => db.Tags.Find(x.Id));
                images = images.Select(x => db.Images.Find(x.Id));
                foreach (var image in images)
                {
                    var relatedTagSets = image.TagSets.Where(x => x.Type == TagSetTypesEnum.UserTypeName
                                                                  || x.Type == TagSetTypesEnum.ActualTypeName);
                    bool isCheck = true;
                    foreach (var tag in tags)
                    {
                        bool tagCheck = false;
                        foreach (var relatedTagSet in relatedTagSets)
                        {
                            if (relatedTagSet.Tags.Contains(tag))
                            {
                                tagCheck = true;
                                break;
                            }
                        }
                        if (!tagCheck)
                        {
                            isCheck = false;
                            break;
                        }
                    }

                    if (isCheck)
                    {
                        result.Add(image);
                    }
                }

                return result;
            }
        }

        public static IEnumerable<Image> Filter(Collection collection, IEnumerable<Tag> tags)
        {
            var images = GetImages(collection);
            return Filter(images, tags);
        }

        #endregion OLD

        #region Collection logic

        public static IEnumerable<Collection> GetCollections()
        {
            using (var db = new ImagesDBConnection())
            {
                return db.Collections;
            }
        }

        public static Collection AddCollection(string name)
        {
            using (var db = new ImagesDBConnection())
            {
                var collection = new Collection {Name = name};
                db.Collections.Add(collection);
                db.SaveChanges();

                return collection;
            }
        }

        public static void RemoveCollection(int collectionId)
        {
            throw new NotImplementedException();
        }

        public static void RenameCollection(int collectionId, string newName)
        {
            throw new NotImplementedException();
        }

        public static void AddSourceToCollection(int collectionId, int sourceId)
        {
            throw new NotImplementedException();
        }

        public static void RemoveSourceFromCollection(int collectionId, int sourceId)
        {
            throw new NotImplementedException();
        }

        #endregion Collection logic

        #region Source logic

        public static IEnumerable<Source> GetSources(Collection collection)
        {
            return GetSources(collection.Id);
        }

        private static IEnumerable<Source> GetSources(int collectionId)
        {
            using (var db = new ImagesDBConnection())
            {
                return db.Collections.Find(collectionId).Sources;
            }
        }

        public static Source CreateSource(string path)
        {
            throw new NotImplementedException();
        }

        public static void UpdateSource(bool checkHashes = false)
        {
            throw new NotImplementedException();
        }

        public static IEnumerable<Source> GetSources()
        {
            throw new NotImplementedException();
        }

        public static Source AddSource(Collection collection, string path)
        {
            using (var db = new ImagesDBConnection())
            {
                var source = new Source(path);

                source = db.Sources.Find(source.Id);
                collection = db.Collections.Find(collection.Id);

                if (collection.Sources.Contains(source))
                {
                    throw new Exception("Collection already contain this source.");
                }

                collection.Sources.Add(source);

                db.SaveChanges();
                return source;
            }
        }

        #endregion Source logic

        #region Image logic

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

        public static void AddTagToImage(int imageId, int tagId)
        {
            throw new NotImplementedException();
        }

        public static void RemoveTagFromImage(int imageId, int tagId)
        {
            throw new NotImplementedException();
        }

        public static IEnumerable<Image> GetImages(int sourceId)
        {
            throw new NotImplementedException();
        }

        public static IEnumerable<Tag> GetTagsFromImage(Image image)
        {
            return image
                .UserTagSets
                .Concat(image.ActualTagSets)
                .Select(x => x.Tags)
                .Aggregate((res, x) => res.Union(x).ToList());
        }

        #endregion Image logic

        #region Tag logic

        public static IEnumerable<Tag> GetTags()
        {
            throw new NotImplementedException();
        }

        public static Tag CreateTag(string name, int tagTypeId)
        {
            throw new NotImplementedException();
        }

        public static void RenameTag(int tagId, string newName)
        {
            throw new NotImplementedException();
        }

        public static void ChangeType(int imageId, int newTagTypeId)
        {
            throw new NotImplementedException();
        }

        #endregion Tag logic

        #region TagType logic

        public static IEnumerable<TagType> GetTagTypes()
        {
            throw new NotImplementedException();
        }

        public static TagType CreateTagType(string name)
        {
            throw new NotImplementedException();
        }

        public static void RemoveTagType(int tagTypeId)
        {
            throw new NotImplementedException();
        }

        public static void RenameTagType(int tagTypeId, string newName)
        {
            throw new NotImplementedException();
        }

        #endregion TagType logic
    }
}
