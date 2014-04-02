using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ImoutoNavigator.Database.Model;
using ImoutoNavigator.Utils;

namespace ImoutoNavigator.Database
{
    class TagsDBWork
    {
        public TagsDBWork()
        {
            //AddFile(@"C:\Users\oniii-chan\Pictures\[Leopard-Raws] Yozakura Quartet - Hana no Uta - 13 END (MX 1280x720 x264 AAC).mp4_snapshot_22.01_[2014.03.14_01.20.24].jpg");
        }

        //public void AddFile(string fullname)
        //{
        //    var fi = new FileInfo(fullname);
        //    if (!fi.Exists)
        //    {
        //        throw new ArgumentException("File does not exist.");
        //    }

        //    string md5 = Util.GetMd5Checksum(fi);

        //    using (var obj = new TagsDBEntities())
        //    {
        //        obj.files.Add(new file()
        //                      {
        //                          fullname = fullname,
        //                          md5 = md5
        //                      });
        //        obj.SaveChanges();
        //    }
        //}

        public static IEnumerable<Image> GetImagesByTags(IEnumerable<Tag> tags)
        {
            var result = new List<Image>();
            foreach (var tag in tags)
            {
                var relatedTagSets = tag.ActualTagSets.Concat(tag.UserTagSets);
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

        public static IEnumerable<Tag> GetTagsTopFromImages(IEnumerable<Image> images, int? count = null)
        {
            IEnumerable<Image> enumerableImages = images as IList<Image> ?? images.ToList();
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
                    .OrderBy(x => x.Count())
                    .Select(x => x.First());

            if (count != null)
            {
                result = result.Take(count.GetValueOrDefault());
            }

            return result;
        }

        public static IEnumerable<Tag> GetTagsFromImage(Image image)
        {
            return image
                .UserTagSets
                .Concat(image.ActualTagSets)
                .Select(x => x.Tags)
                .Aggregate((res, x) => res.Union(x).ToList());
        }

        public static void AddOrCorrectPathForImagesIfNotExist(IEnumerable<Image> images)
        {
            using (var db = new ImagesDBConnection())
            {
                var enumerableImages = images as IList<Image> ?? images.ToList();
                var newImages = enumerableImages;
                var dbimages = db.Images;
                foreach (var image in enumerableImages)
                {
                    foreach (var dbimage in dbimages.Where(dbimage => dbimage.Md5 == image.Md5
                                                                       && dbimage.Size == image.Size))
                    {
                        dbimage.Path = image.Path;
                        (newImages = newImages.ToList()).Remove(image);
                        break;
                    }
                }

                db.Images.AddRange(newImages);

                db.SaveChanges();
            }
        }

        public static void AddOrCorrectPathForImagesIfNotExist(IEnumerable<string> imagePaths)
        {
            var images = imagePaths.Select(x => new Image(x));
            AddOrCorrectPathForImagesIfNotExist(images);
        }

        public static void AddTagsIfNotExist(IEnumerable<Tag> tags)
        {
            using (var db = new ImagesDBConnection())
            {
                var enumerableList = tags as IList<Tag> ?? tags.ToList();
                var newTags = enumerableList;
                foreach (var tag in enumerableList)
                {
                    foreach (Tag dbtag in db.Tags)
                    {
                        if (dbtag.Name == tag.Name && dbtag.TagType == tag.TagType)
                        {
                            (newTags = newTags.ToList()).Remove(tag);
                            break;
                        }
                    }
                }

                db.Tags.AddRange(newTags);

                db.SaveChanges();
            }
        }

        public static void AddNewTagsToImages(IEnumerable<Tag> tags, IEnumerable<Image> images)
        {
            if (!tags.Any() || !images.Any())
            {
                throw new ArgumentException("Collection of tags or images is empty");
            }

            //AddTagsIfNotExist(tags);
            AddOrCorrectPathForImagesIfNotExist(images);

            foreach (var image in images)
            {
                var userTagSets = image.UserTagSets;

                var userTagSet = userTagSets.Any()
                    ? userTagSets.First()
                    : new TagSet(TagSetTypesEnum.UserTypeName, image);


                var newTags = userTagSet.Tags;
                if (newTags.Count > 0)
                {
                    tags = tags.Union(newTags);
                }
                userTagSet.Tags = tags.ToList();
            }

            using (var db = new ImagesDBConnection())
            {
                db.SaveChanges();
            }
        }

        public static void RemoveTagsFromImages(IEnumerable<Tag> tags, IEnumerable<Image> images)
        {
            if (!tags.Any() || !images.Any())
            {
                throw new ArgumentException("Collection of tags or images is empty");
            }

            foreach (var image in images.Where(x => x.UserTagSets.Any()))
            {
                foreach (var tag in tags)
                {
                    image.UserTagSets.First().Tags.Remove(tag);
                }
            }

            using (var db = new ImagesDBConnection())
            {
                db.SaveChanges();
            }
        }
    }
}
