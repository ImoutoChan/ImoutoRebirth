using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace UtilsTest
{
    class Program
    {
        private static List<FileEntry> fileList = new List<FileEntry>();

        private class FileEntry
        {
            public string Md5 { get; set; }

            public FileInfo File { get; set; }

            public long Size { get; set; }

            public long Time { get; set; }
        }

        static void Main(string[] args)
        {
            foreach (var file in Directory.GetFiles(@"T:\art").Select(x => new FileInfo(x)))
            {
                var t = new Stopwatch();
                t.Start();
                fileList.Add(new FileEntry() {Md5 = Util.GetMd5Checksum(file), Size = file.Length, File = file});
                fileList.Last().Time = t.ElapsedMilliseconds;
                t.Stop();
                t = null;
            }

            using (var sw = new StreamWriter(@"log.txt"))
            {
                foreach (var fileEntry in fileList.OrderByDescending(x=>x.Size))
                {
                    sw.WriteLine("{0}\t{1}", fileEntry.Size, fileEntry.Time);
                }
            }
        }
    }
}
