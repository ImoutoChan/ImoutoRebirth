using System;
using System.Linq;
using ImagesDBLibrary.Model;
using System.Collections.Generic;

namespace DBConnection
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //CollectionM collection1;

                //if (!CollectionM.Collections.Any())
                //{
                //    collection1 = CollectionM.Create("Collection1");
                //}
                //else
                //{
                //    collection1 = CollectionM.Collections.First();
                //}


                //if (!collection1.Sources.Any())
                //{
                //    collection1.AddSource(@"C:\Users\Владимир\Downloads\Обои");
                //    collection1.AddSource(@"C:\Users\Владимир\Downloads\Обои\Test");
                //}

                //collection1.Activate();

                //foreach (var image in collection1.Images)
                //{
                //    Console.WriteLine(image.Path);
                //}

                //Console.ReadKey();

                List<TagM> tags;
                CollectionM collection1;
                List<ImageM> images;

                if (CollectionM.Collections.Count == 0)
                {
                    var st2 = DateTime.Now;

                    collection1 = CollectionM.Create("Collection1");
                    Console.WriteLine("Creating coollection1 : " + (DateTime.Now - st2).TotalMilliseconds);

                    var collection2 = CollectionM.Create("Collection2");
                    Console.WriteLine("Creating coollection2 : " + (DateTime.Now - st2).TotalMilliseconds);

                    Console.Write("\nTimer reset.\n\n");
                    st2 = DateTime.Now;

                    collection1.AddSource(@"C:\Users\oniii-chan\Downloads\temp\source1");
                    collection1.AddSource(@"C:\Users\oniii-chan\Downloads\temp\source2");
                    collection2.AddSource(@"C:\Users\oniii-chan\Downloads\temp\source2");
                    collection2.AddSource(@"C:\Users\oniii-chan\Downloads\temp\source3");

                    //collection1.AddSource(@"C:\Users\Владимир\Downloads\Обои\");
                    //collection1.AddSource(@"C:\Users\oniii-chan\Downloads\temp\");

                    Console.WriteLine("Add source 'Обои' to collection1 : " + (DateTime.Now - st2).TotalMilliseconds);

                    Console.Write("\nTimer reset.\n\n");
                    st2 = DateTime.Now;

                    //collection2.AddSource(@"C:\Users\Владимир\Downloads\Обои\Test");
                    //Console.WriteLine("Add source 'Обои' to collection2 : " + (DateTime.Now - st2).TotalMilliseconds);

                    Console.Write("Timer reset.\n\n");
                    st2 = DateTime.Now;

                    collection2.Activate();
                    Console.WriteLine("Collection 2 was activated in : " + (DateTime.Now - st2).TotalMilliseconds);

                    Console.Write("Timer reset.\n\n");
                    st2 = DateTime.Now;

                    collection1.Activate();
                    Console.WriteLine("Collection 1 was activated in : " + (DateTime.Now - st2).TotalMilliseconds);

                    Console.Write("Timer reset.\n\n");
                    st2 = DateTime.Now;

                    collection2.Activate();
                    Console.WriteLine("Collection 2 was reactivated in : " + (DateTime.Now - st2).TotalMilliseconds);


                    Console.Write("Timer reset.\n\n");
                    st2 = DateTime.Now;


                    //collection2.RemoveSource(
                    //                         collection2
                    //                             .Sources
                    //                             .First(x => x.Path ==
                    //                                         @"C:\Users\oniii-chan\Downloads\temp\source2"));

                    Console.WriteLine((DateTime.Now - st2).TotalMilliseconds);
                    collection2.Activate();

                    collection1.Activate();

                    collection2.Activate();

                    Console.WriteLine((DateTime.Now - st2).TotalMilliseconds);
                    //collection1.Remove();

                    Console.WriteLine((DateTime.Now - st2).TotalMilliseconds);
                    collection2.Rename("Collection2_renamed");

                    Console.WriteLine((DateTime.Now - st2).TotalMilliseconds);
                    var collection1new = CollectionM.Create("Collection1_");

                    Console.WriteLine((DateTime.Now - st2).TotalMilliseconds);
                    st2 = DateTime.Now;

                    images = collection1.GetImages(30);

                    Console.WriteLine("getting image in : " + (DateTime.Now - st2).TotalMilliseconds);
                    st2 = DateTime.Now;

                    var testType = TagTypeM.Create("TestType");
                    var tag1 = TagM.Create("tag1", testType);
                    var tag2 = TagM.Create("tag2", testType);
                    var tag3 = TagM.Create("tag3", testType);
                    var tag4 = TagM.Create("tag4", testType);

                    tags = new List<TagM>();
                    for (int i = 0; i < 11; i++)
                    {
                        tags.Add(TagM.Create("tagNumber" + i, testType));
                    }

                    Console.WriteLine((DateTime.Now - st2).TotalMilliseconds);
                    st2 = DateTime.Now;

                    var random = new Random();
                    foreach (var image in images)
                    {
                        var alreadyAddedTags = new List<TagM>();
                        for (int i = 0; i < random.Next(1, 10); i++)
                        {
                            var tag = tags[random.Next(0, 10)];
                            if (!alreadyAddedTags.Contains(tag))
                            {
                                alreadyAddedTags.Add(tag);
                                image.AddTag(tag);
                            }
                        }
                    }

                    Console.WriteLine(@"tags added in: " + (DateTime.Now - st2).TotalMilliseconds);
                    st2 = DateTime.Now;

                    foreach (var image in images)
                    {
                        Console.WriteLine("{0} ; tags: {1}\n\n", image.Path.Substring(image.Path.Length - 10), String.Join(", ", image.Tags.Select(x => x.Name)));
                    }

                    Console.WriteLine((DateTime.Now - st2).TotalMilliseconds);
                    st2 = DateTime.Now;
                }
                else
                {
                    collection1 = CollectionM.Collections.First();
                    tags = TagM.Tags.Skip(TagM.Tags.Count - 10).Take(10).ToList();
                }

                int count = collection1.CountImagesWithTags(new List<TagM> { tags[5] });

                var images1 = collection1.GetImages(10, 0, new List<TagM> { tags[5] });

                foreach (var image in images1)
                {
                    Console.WriteLine("{0} ; tags: {1}\n\n", image.Path.Split('\\').Last(), String.Join(", ", image.Tags.Select(x => x.Name)));
                    
                }


                //images.First().AddTag(tag1);
                //images.Last().AddTag(tag2);

                //images.First().RemoveTag(tag1);
                ////collection2.Images.First().RemoveTag(tag2);

                //tag2.Rename("tag2_renamed");
                //tag2.ChangeType(TagTypeM.Create("TestTypeSecond"));

                //TagTypeM.TagTypes.Find(x => x.Name.EndsWith("d")).Rename("TestTypeSecond_renamed");
                //TagTypeM.Create("TempTagType").Remove();

                //Console.WriteLine((DateTime.Now - st2).TotalMilliseconds);
                Console.ReadKey();
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
