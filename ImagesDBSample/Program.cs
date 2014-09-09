using System;
using System.Linq;
using ImagesDBLibrary.Model;

namespace DBConnection
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {

                var st2 = DateTime.Now;
                var collection1 = CollectionM.Create("Collection1");
                Console.WriteLine("Creating coollection1 :" + (DateTime.Now - st2).TotalMilliseconds);
                var collection2 = CollectionM.Create("Collection2");
                Console.WriteLine("Creating coollection2 :" + (DateTime.Now - st2).TotalMilliseconds);

                Console.Write("\nTimer reset.\n\n");
                st2 = DateTime.Now;
                //collection1.AddSource(@"C:\Users\oniii-chan\Downloads\temp\source1");
                //collection1.AddSource(@"C:\Users\oniii-chan\Downloads\temp\source2");
                //collection2.AddSource(@"C:\Users\oniii-chan\Downloads\temp\source2");
                //collection2.AddSource(@"C:\Users\oniii-chan\Downloads\temp\source3");
                
                collection1.AddSource(@"C:\Users\Владимир\Downloads\Обои");
                Console.WriteLine("Add source 'Обои' to collection1" + (DateTime.Now - st2).TotalMilliseconds);

                Console.Write("\nTimer reset.\n\n");
                st2 = DateTime.Now;

                collection2.AddSource(@"C:\Users\Владимир\Downloads\Обои");
                Console.WriteLine("Add source 'Обои' to collection2" + (DateTime.Now - st2).TotalMilliseconds);


                Console.Write("\nTimer reset.\n\n");
                st2 = DateTime.Now;

                collection2.Activate();

                collection1.Activate();

                collection2.Activate();

                Console.WriteLine((DateTime.Now - st2).TotalMilliseconds);


                collection2.RemoveSource(
                                         collection2
                                             .Sources
                                             .First(x => x.Path ==
                                                         @"C:\Users\oniii-chan\Downloads\temp\source2"));

                Console.WriteLine((DateTime.Now - st2).TotalMilliseconds);
                collection2.Activate();

                collection1.Activate();

                collection2.Activate();

                Console.WriteLine((DateTime.Now - st2).TotalMilliseconds);
                collection1.Remove();

                Console.WriteLine((DateTime.Now - st2).TotalMilliseconds);
                collection2.Rename("Collection2_renamed");

                Console.WriteLine((DateTime.Now - st2).TotalMilliseconds);
                var collection1new = CollectionM.Create("Collection1");

                Console.WriteLine((DateTime.Now - st2).TotalMilliseconds);

                st2 = DateTime.Now;
                foreach (var image in collection2.Images)
                {
                    Console.WriteLine(image.Path);
                }
                Console.WriteLine((DateTime.Now - st2).TotalMilliseconds);
                st2 = DateTime.Now;

                var testType = TagTypeM.Create("TestType");
                var tag1 = TagM.Create("tag1", testType);
                var tag2 = TagM.Create("tag2", testType);
                var tag3 = TagM.Create("tag3", testType);
                var tag4 = TagM.Create("tag4", testType);

                collection2.Images.First().AddTag(tag1);
                collection2.Images.Last().AddTag(tag2);

                collection2.Images.First().RemoveTag(tag1);
                //collection2.Images.First().RemoveTag(tag2);

                tag2.Rename("tag2_renamed");
                tag2.ChangeType(TagTypeM.Create("TestTypeSecond"));

                TagTypeM.TagTypes.Find(x => x.Name.EndsWith("d")).Rename("TestTypeSecond_renamed");
                TagTypeM.Create("TempTagType").Remove();
            }
            catch (Exception)
            {
                throw;
            }
            /*
             * 
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
            */
            Console.ReadLine();
        }
    }
}
