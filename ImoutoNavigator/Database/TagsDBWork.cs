using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
    }
}
