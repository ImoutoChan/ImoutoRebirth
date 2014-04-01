using System;
using System.IO;
using ImoutoNavigator.Utils;

namespace ImoutoNavigator.Database.Model
{
    public partial class Image
    {
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
    }
}
