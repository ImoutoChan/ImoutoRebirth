using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace ImagesDBLibrary.Database.Model
{
    public static class TagSetTypesEnum
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

        public Image(string path) : this()
        {
            var fileInfo = new FileInfo(path);
            if (!fileInfo.Exists)
            {
                throw new ArgumentException("File does not exist.");
            }
            Path = path;

            using (var db = new ImagesDBConnection())
            {
                Md5 = GetMd5Checksum(fileInfo);
                Size = fileInfo.Length;

                var dbimg = db.Images.FirstOrDefault(x => x.Path == Path);
                if (dbimg != null)
                {
                    //throw new ArgumentException("Image already in base");
                    
                    Id = dbimg.Id;
                }
                else
                {
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

        private static string GetMd5Checksum(FileInfo targetFile)
        {
            if (!targetFile.Exists)
            {
                throw new ArgumentException("File does not exist.");
            }

            using (var md5 = MD5.Create())
            {
                using (var stream = targetFile.OpenRead())
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                }
            }
        }
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
}
