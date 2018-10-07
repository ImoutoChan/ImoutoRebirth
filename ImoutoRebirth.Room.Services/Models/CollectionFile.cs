using System;
using ImoutoRebirth.Room.Core.Models.Abstract;

namespace ImoutoRebirth.Room.Core.Models
{
    public class CollectionFile : ModelBase
    {
        public Guid CollectionId { get; }

        public string Path { get; }

        public string Md5 { get; }

        public long Size { get; }

        public string OriginalPath { get; }

        public CollectionFile(
            Guid id,
            Guid collectionId,
            string path,
            string md5,
            long size,
            string originalPath) 
            : base(id)
        {
            CollectionId = collectionId;
            Path = path;
            Md5 = md5;
            Size = size;
            OriginalPath = originalPath;
        }
    }
}