namespace ImoutoRebirth.Room.WebApi.Responses
{
    public class CollectionFileResponse
    {
        public Guid Id { get; }

        public Guid CollectionId { get; }

        public string Path { get; }

        public string Md5 { get; }

        public long Size { get; }

        public string OriginalPath { get; }

        public CollectionFileResponse(
            Guid id,
            Guid collectionId,
            string path,
            string md5,
            long size,
            string originalPath)
        {
            Id = id;
            CollectionId = collectionId;
            Path = path;
            Md5 = md5;
            Size = size;
            OriginalPath = originalPath;
        }
    }
}