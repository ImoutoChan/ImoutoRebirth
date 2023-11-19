namespace ImoutoRebirth.Room.Domain;

public class CollectionFile
{
    public CollectionFile(
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
        Md5 = md5.ToLowerInvariant();
        Size = size;
        OriginalPath = originalPath;
    }

    public Guid Id { get; }

    public Guid CollectionId { get; }

    public string Path { get; }

    public string Md5 { get; }

    public long Size { get; }

    public string OriginalPath { get; }
}
