using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ImoutoNavigator.Utils;

namespace ImoutoNavigator.Database
{
    class TagsDBWork
    {
        public TagsDBWork()
        {
            //AddFile(@"C:\Users\oniii-chan\Pictures\[Leopard-Raws] Yozakura Quartet - Hana no Uta - 13 END (MX 1280x720 x264 AAC).mp4_snapshot_22.01_[2014.03.14_01.20.24].jpg");
        }

        public void AddFile(string fullname)
        {
            var fi = new FileInfo(fullname);
            if (!fi.Exists)
            {
                throw new ArgumentException("File does not exist.");
            }

            string md5 = Util.GetMd5Checksum(fi);

            using (var obj = new TagsDBEntities())
            {
                obj.files.Add(new file()
                              {
                                  fullname = fullname,
                                  md5 = md5
                              });
                obj.SaveChanges();
            }
        }
    }
}
