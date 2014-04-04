using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBConnection.Database;
using DBConnection.Database.Model;

namespace DBConnection
{
    class Program
    {
        static void Main(string[] args)
        {

            var st1 = DateTime.Now;
            TagsDBWork.AddOrCorrectPathForImagesIfNotExist(
                Directory.GetFiles(@"C:\Users\Владимир\Downloads\Обои\Обои\Замки"));
            Console.WriteLine((st1 - DateTime.Now).TotalMilliseconds);

            using (var db = new ImagesDBConnection())
            {
                //db.TagTypes.Add(new TagType() {Name = "copyright"});
                //db.SaveChanges();

                List<Tag> list = new List<Tag>
                {
                    new Tag("test1", db.TagTypes.First().Id),
                    new Tag("test2", db.TagTypes.First().Id),
                    new Tag("test1", db.TagTypes.First().Id),
                };

                TagsDBWork.AddNewTagsToImages(list, db.Images);
            }

            TagsDBWork.AddOrCorrectPathForImagesIfNotExist(
                Directory.GetFiles(@"C:\Users\Владимир\Downloads\Обои\Обои\Замки"));

            using (var db = new ImagesDBConnection())
            {
                //db.TagTypes.Add(new TagType() {Name = "copyright"});
                //db.SaveChanges();

                List<Tag> list = new List<Tag>
                {
                    new Tag("test1", db.TagTypes.First().Id),
                    new Tag("test2", db.TagTypes.First().Id),
                    new Tag("test1", db.TagTypes.First().Id),
                };

                TagsDBWork.AddNewTagsToImages(list, db.Images);
            }
        }
    }
}
