using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace DBConnection.Database.Model
{
    public static class TagSetTypesEnum
    {
        public const string UserTypeName = "User";
        public const string ActualTypeName = "Actual";
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

            Md5 = "aaaa aaaa aaaa aaaa aaaa aaaa aa";// GetMd5Checksum(fileInfo);
            Path = path;
            Size = fileInfo.Length;
        }

        #endregion Constructors

        #region Properties

        public IEnumerable<TagSet> UserTagSets
        {
            get { return TagSets.Where(x => x.TagSetType.Name == TagSetTypesEnum.UserTypeName); }
        }

        public IEnumerable<TagSet> ActualTagSets
        {
            get { return TagSets.Where(x => x.TagSetType.Name == TagSetTypesEnum.ActualTypeName); }
        }

        #endregion Properties 

        public static string GetMd5Checksum(FileInfo targetFile)
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

        public Tag(string name, int tagType) : this()
        {
            using (var db = new ImagesDBConnection())
            {
                if (db.Tags.Any(x => x.Name == name && x.Type == tagType))
                {
                    return;
                }

                Name = name;
                Type = tagType;

                db.Tags.Add(this);
                db.SaveChanges();
            }
        }

        #endregion Constructor

        #region Properties

        public IEnumerable<TagSet> UserTagSets
        {
            get { return TagSets.Where(x => x.TagSetType.Name == TagSetTypesEnum.UserTypeName); }
        }

        public IEnumerable<TagSet> ActualTagSets
        {
            get { return TagSets.Where(x => x.TagSetType.Name == TagSetTypesEnum.ActualTypeName); }
        }

        #endregion Properties 
    }

    public partial class TagSet
    {
        #region Constructor

        public TagSet(string type, Image image) : this()
        {
            using (var db = new ImagesDBConnection())
            {
                var typeObj = db.TagSetTypes.First(x => x.Name == type);
                if (type == null)
                {
                    throw new ArgumentException("Incorrect TagSetType.");
                }
                FKType = typeObj.Id;

                this.AddedDate = DateTime.Now;
                this.FKImage = image.Id;

                db.TagSets.Add(this);
                db.SaveChanges();
            }
        }

        #endregion Constructor
    }
}
