using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using EntityFrameworkExtras;
using Utils;

namespace ImagesDBLibrary.Database.Model
{
    internal static class TagSetTypesEnum
    {
        public const string UserTypeName = "User";
        public const string ActualTypeName = "Actual";
    }

    public enum TagSetTypes
    {
        User = 3,
        Actual = 2,
        None = 1
    }

    public enum TagTypes
    {
        Artist = 3,
        Character = 2,
        Copyright = 1
    }
    public partial class Image
    {
        #region Constructors

        public Image(string path, bool CheckOnExist = true) : this()
        {
            var fileInfo = new FileInfo(path);
            if (CheckOnExist && !fileInfo.Exists)
            {
                throw new ArgumentException("File does not exist.");
            }
            Path = path;

            using (var db = new ImagesDBConnection())
            {
                var dbimg = db.Images.FirstOrDefault(x => x.Path == Path);
                if (dbimg != null)
                {
                    //throw new ArgumentException("Image already in base");
                    
                    Id = dbimg.Id;
                    Md5 = dbimg.Md5;
                    Size = dbimg.Size;
                }
                else
                {
                    Md5 = Util.GetMd5Checksum(fileInfo);
                    Size = fileInfo.Length;

                    //this saves in db with tagset
                    db.TagSets.Add(new TagSet(TagSetTypesEnum.UserTypeName, this));
                    db.SaveChanges();
                }
            }
        }

        #endregion Constructors

        #region Properties

        public IEnumerable<TagSet> UserTagSets
        {
            get { return TagSets.Where(x => x.Type == TagSetTypesEnum.UserTypeName); }
        }

        public IEnumerable<TagSet> ActualTagSets
        {
            get { return TagSets.Where(x => x.Type == TagSetTypesEnum.ActualTypeName); }
        }

        #endregion Properties 
    }

    public partial class Tag
    {
        #region Constructor

        public Tag(string name, TagTypes tagType)
            : this()
        {
            Name = name;
            Type = (int)tagType;

            using (var db = new ImagesDBConnection())
            {
                var dbtag = db.Tags.FirstOrDefault(x => x.Name == Name && x.Type == (int) tagType);
                if (dbtag != null)
                {
                    //throw new ArgumentException("Tag already in base");
                    Id = dbtag.Id;
                }
                else
                {
                    db.Tags.Add(this);
                    db.SaveChanges();
                }

            }
        }

        #endregion Constructor

        #region Properties

        public IEnumerable<TagSet> UserTagSets
        {
            get { return TagSets.Where(x => x.Type == TagSetTypesEnum.UserTypeName); }
        }

        public IEnumerable<TagSet> ActualTagSets
        {
            get { return TagSets.Where(x => x.Type == TagSetTypesEnum.ActualTypeName); }
        }

        #endregion Properties 
    }

    public partial class TagSet
    {
        #region Constructor

        public TagSet(string type, Image image) : this()
        {
            Type = type;
            AddedDate = DateTime.Now;
            Image = image;
        }

        #endregion Constructor
    }

    public partial class Source
    {
        public Source(string path)
        {
            var di = new DirectoryInfo(path);
            if (!di.Exists)
            {
                throw new ArgumentException("Directory does not exist.");
            }

            using (var db = new ImagesDBConnection())
            {
                Path = path;

                var dbsource = db.Sources.FirstOrDefault(x => x.Path == path);
                if (dbsource != null)
                {
                    //throw new ArgumentException("Source already exist");
                    Id = dbsource.Id;
                }
                else
                {
                    var files = new List<FileInfo>();

                    files.AddRange(di.GetFiles().Where(x => x.FullName.IsImage()));
                    foreach (var dir in di.GetDirectories(isRecursive: true))
                    {
                        files.AddRange(di.GetFiles().Where(x=>x.FullName.IsImage()));
                    }

                    var images = files.Select(x => new Image(x.FullName, false));

                    images = images.Select(x => db.Images.Find(x.Id));

                    db.Sources.Add(this);
                    db.SaveChanges();

                    var thiss = db.Sources.Include(x=>x.Images).Single(x=>x.Id == Id);

                    foreach (var image in images)
                    {
                        thiss.Images.Add(image);
                    }
                    
                    db.SaveChanges();
                }
            }
        }
    }
}
