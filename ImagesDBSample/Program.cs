using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ImagesDBLibrary.Database.Model;
using ImagesDBLibrary.Database;

namespace DBConnection
{
    class Program
    {
        static void Main(string[] args)
        {
            var st2 = DateTime.Now;
            ImagesDB.AddSource(ImagesDB.AddCollection("Lol"), @"C:\Users\Владимир\Downloads\Обои\Обои\Замки");
            Console.WriteLine((DateTime.Now - st2).TotalMilliseconds); 

            //using (var db = new ImagesDBConnection())
            //{
            //    var st2 = DateTime.Now;

            //    var list = new List<Tag>
            //    {
            //        new Tag("te5321st1", TagTypes.Artist),
            //        new Tag("test1", TagTypes.Artist),
            //        new Tag("test2", TagTypes.Artist),
            //        new Tag("test3", TagTypes.Artist),
            //        new Tag("test6", TagTypes.Artist),
            //        new Tag("test7", TagTypes.Artist),
            //        new Tag("test8", TagTypes.Artist),

            //        new Tag("1", TagTypes.Artist),
            //        new Tag("2", TagTypes.Copyright),
            //        new Tag("3", TagTypes.Artist),
            //        new Tag("te321st1", TagTypes.Artist),
            //        new Tag("t2112323est2", TagTypes.Artist),
            //        new Tag("te213s31t1", TagTypes.Artist),
            //        new Tag("t354est2", TagTypes.Artist),
            //        new Tag("4", TagTypes.Artist),
            //        new Tag("5", TagTypes.Copyright),

            //        new Tag("tes23t1", TagTypes.Artist),
            //        new Tag("tes12323t2", TagTypes.Artist),
            //        new Tag("tes123t1", TagTypes.Artist),
            //        new Tag("te31245st2", TagTypes.Artist),
            //        new Tag("te31245st2", TagTypes.Character)
            //    };

            //    Console.WriteLine((DateTime.Now - st2).TotalMilliseconds);
            //    st2 = DateTime.Now;

            //    ImagesDB.AddTagsToImages(
            //        list,
            //        Directory.GetFiles(@"C:\Users\oniii-chan\Downloads\DLS\").Select(x => new Image(x)).ToList());

            //    Console.WriteLine((DateTime.Now - st2).TotalMilliseconds);
            //    st2 = DateTime.Now;

            //    ImagesDB.AddTagsToImages(
            //        list,
            //        Directory.GetFiles(@"C:\Users\oniii-chan\Downloads\DLS\").Select(x => new Image(x)).ToList());

            //    Console.WriteLine((DateTime.Now - st2).TotalMilliseconds);
            //    st2 = DateTime.Now;

            //    foreach (var tag in ImagesDB.GetTagsFromImage(db.Images.First()))
            //    {
            //        Console.WriteLine(tag.Name);
            //    }

            //    Console.WriteLine((DateTime.Now - st2).TotalMilliseconds); 
            //    st2 = DateTime.Now;

            //    foreach (var tag in ImagesDB.GetTagsTopFromImages(db.Images, 5))
            //    {
            //        Console.WriteLine("{0} ({1})",tag.Key.Name, tag.Value);
            //    }

            //    Console.WriteLine((DateTime.Now - st2).TotalMilliseconds);
            //    st2 = DateTime.Now;

            //    foreach (var image in ImagesDB.GetImagesByTags(list.First()))
            //    {
            //        //Console.WriteLine(image.Path);
            //    }

            //    Console.WriteLine((DateTime.Now - st2).TotalMilliseconds);
            //    st2 = DateTime.Now;

            //    ImagesDB.RemoveTagsFromImages(list, db.Images.ToList());

            //    Console.WriteLine((DateTime.Now - st2).TotalMilliseconds);
            //    st2 = DateTime.Now;
            //}

            Console.ReadLine();
        }
    }
}
