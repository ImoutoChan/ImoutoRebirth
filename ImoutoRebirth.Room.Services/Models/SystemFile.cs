namespace ImoutoRebirth.Room.Core.Models
{
    public class SystemFile
    {
        public FileInfo File { get; }

        public string Md5 { get; }

        public long Size { get; }

        public SystemFile(
            FileInfo file,
            string md5,
            long size)
        {
            File = file;
            Md5 = md5;
            Size = size;
        }
    }
}