using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ImoutoNavigator.Utils;

namespace ImoutoNavigator.Database.Model
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

            Md5 = Util.GetMd5Checksum(fileInfo);
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
    }

    public partial class Tag
    {
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
}
