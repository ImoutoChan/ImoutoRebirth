using System;
using System.Collections.Generic;
using System.Linq;
using ImagesDBLibrary.Database.Model;

namespace ImagesDBLibrary.Database.Access
{
    internal static partial class ImagesDB
    {
        #region OLD
        /*
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
        */
        #endregion OLD

        #region Collection logic

        public static List<Collection> GetCollections()
        {
            using (var db = new ImagesDBConnection())
            {
                return db.Collections.ToList();
            }
        }

        public static Collection CreateCollection(string name)
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
            using (var db = new ImagesDBConnection())
            {
                var collection = db.Collections.Find(collectionId);
                collection.Sources.Clear();
                db.Collections.Remove(db.Collections.Find(collectionId));
                db.SaveChanges();
            }
        }

        public static void RenameCollection(int collectionId, string newName)
        {
            using (var db = new ImagesDBConnection())
            {
                db.Collections.Find(collectionId).Name = newName;
                db.SaveChanges();
            }
        }

        public static void AddSourceToCollection(int collectionId, int sourceId)
        {
            using (var db = new ImagesDBConnection())
            {
                var source = db.Sources.Find(sourceId);
                var collection = db.Collections.Find(collectionId);

                collection.Sources.Add(source);
                db.SaveChanges();
            }
        }

        public static void RemoveSourceFromCollection(int collectionId, int sourceId)
        {
            using (var db = new ImagesDBConnection())
            {
                var source = db.Sources.Find(sourceId);
                var collection = db.Collections.Find(collectionId);

                collection.Sources.Remove(source);
                db.SaveChanges();
            }
        }

        #endregion Collection logic

        #region Source logic

        public static List<Source> GetSources(Collection collection)
        {
            return GetSources(collection.Id);
        }

        private static List<Source> GetSources(int collectionId)
        {
            using (var db = new ImagesDBConnection())
            {
                return db.Collections.Find(collectionId).Sources.ToList();
            }
        }

        public static List<Source> GetSources()
        {
            using (var db = new ImagesDBConnection())
            {
                return db.Sources.ToList();
            }
        }

        public static Source CreateSource(string path)
        {
            using (var db = new ImagesDBConnection())
            {
                var source = new Source(path);
                //db.Sources.Add(source);
                //db.SaveChanges();

                return source;
            }
        }

        //todo
        public static void UpdateSource(bool checkHashes = false)
        {
            throw new NotImplementedException();
        }

        #endregion Source logic

        #region Image logic

        #region OLD
        /*
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
        */
        #endregion OLD

        public static void AddTagToImage(int imageId, int tagId)
        {
            using (var db = new ImagesDBConnection())
            {
                var image = db.Images.Find(imageId);
                var tag = db.Tags.Find(tagId);

                image.UserTagSets.First().Tags.Add(tag);
                db.SaveChanges();
            }
        }

        public static void AddTagsToImage(int imageId, IEnumerable<int> tagsId)
        {
            using (var db = new ImagesDBConnection())
            {
                var tags = tagsId
                        .Select(x => db.Tags.Find(x))
                        .ToList();

                var userTagSet = db
                    .TagSets
                    .First(x => x.Type == TagSetTypesEnum.UserTypeName && x.FKImage == imageId);

                foreach (var tag in tags)
                {
                    userTagSet
                        .Tags
                        .Add(tag);
                }

                db.SaveChanges();
            }
        }

        public static void RemoveTagFromImage(int imageId, int tagId)
        {
            using (var db = new ImagesDBConnection())
            {
                var image = db.Images.Find(imageId);
                var tag = db.Tags.Find(tagId);

                image.UserTagSets.First().Tags.Remove(tag);
                db.SaveChanges();
            }
        }

        public static List<Image> GetImages(int sourceId)
        {
            using (var db = new ImagesDBConnection())
            {
                var source = db.Sources.Find(sourceId);

                return source.Images.ToList();
            }
        }

        public static List<Tag> GetTagsFromImage(Image image)
        {
            using (var db = new ImagesDBConnection())
            {
                var dbimage = db.Images.Find(image.Id);

                //return dbimage.TagSets.Where(x => x.Type == TagSetTypesEnum.ActualTypeName || x.Type == TagSetTypesEnum.UserTypeName).SelectMany(x => x.Tags).Distinct().ToList();
                return dbimage.TagSets.SelectMany(x => x.Tags).Distinct().ToList();
            }
        }

        public static Dictionary<Image, List<Tag>> GetTagsForImages(List<Image> images)
        {
            var result = new Dictionary<Image, List<Tag>>();

            using (var db = new ImagesDBConnection())
            {
                foreach (var image in images)
                {
                    var dbimage = db.Images.Find(image.Id);

                    result.Add(image, dbimage.TagSets.Where(x => x.Type == TagSetTypesEnum.ActualTypeName || x.Type == TagSetTypesEnum.UserTypeName).SelectMany(x => x.Tags).Distinct().ToList());
                }
            }

            return result;
        }

        public static Dictionary<Image, List<Tag>> GetTagsForImagesUsingSQL(List<Image> images)
        {
            var result = new Dictionary<Image, List<Tag>>();

            using (var db = new ImagesDBConnection())
            {
                foreach (var image in images)
                {                    
                    var tags = db.Database.SqlQuery<int>(@"     
                            SELECT t.Id 
                            FROM Tag t
                            INNER JOIN TagsInTagSet tits ON tits.Tag = t.Id
                            INNER JOIN TagSet ts ON ts.FKImage = @p0", image.Id);                   

                    //var dbimage = db.Images.Find(image.Id);
                    //result.Add(image, dbimage.TagSets.Where(x => x.Type == TagSetTypesEnum.ActualTypeName || x.Type == TagSetTypesEnum.UserTypeName).SelectMany(x => x.Tags).Distinct().ToList());
                    result.Add(image, db.Tags.Where(x => tags.Contains(x.Id)).ToList());
                }
            }

            return result;
        }

        #endregion Image logic

        #region Tag logic

        public static List<Tag> GetTags()
        {
            using (var db = new ImagesDBConnection())
            {
                return db.Tags.ToList();
            }
        }

        public static Tag CreateTag(string name, int tagTypeId)
        {
            using (var db = new ImagesDBConnection())
            {
                var tag = new Tag() {Name = name, Type = tagTypeId};
                db.Tags.Add(tag);
                db.SaveChanges();

                return tag;
            }
        }

        public static void RenameTag(int tagId, string newName)
        {
            using (var db = new ImagesDBConnection())
            {
                db.Tags.Find(tagId).Name = newName;
                db.SaveChanges();
            }
        }

        public static void ChangeType(int tagId, int newTagTypeId)
        {
            using (var db = new ImagesDBConnection())
            {
                db.Tags.Find(tagId).Type = newTagTypeId;
                db.SaveChanges();
            }
        }

        #endregion Tag logic

        #region TagType logic

        public static List<TagType> GetTagTypes()
        {
            using (var db = new ImagesDBConnection())
            {
                return db.TagTypes.ToList();
            }
        }

        public static TagType CreateTagType(string name)
        {
            using (var db = new ImagesDBConnection())
            {
                var tagType = new TagType {Name = name};
                db.TagTypes.Add(tagType);
                db.SaveChanges();
                return tagType;
            }
        }

        public static void RemoveTagType(int tagTypeId)
        {
            using (var db = new ImagesDBConnection())
            {
                var tagType = db.TagTypes.Find(tagTypeId);
                db.TagTypes.Remove(tagType);
                db.SaveChanges();
            }
        }

        public static void RenameTagType(int tagTypeId, string newName)
        {
            using (var db = new ImagesDBConnection())
            {
                db.TagTypes.Find(tagTypeId).Name = newName;
                db.SaveChanges();
            }
        }

        #endregion TagType logic
    }
}
