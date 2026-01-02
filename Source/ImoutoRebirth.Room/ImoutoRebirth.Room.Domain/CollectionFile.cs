using ImoutoRebirth.Common;

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

    public string Path { get; private set; }

    public string Md5 { get; }

    public long Size { get; }

    public string OriginalPath { get; }

    public void Rename(string newFileName)
    {
        var directory = System.IO.Path.GetDirectoryName(Path);

        if (directory == null)
            throw new DomainException("Can't determine directory for new file path.");

        Path = System.IO.Path.Combine(directory, newFileName);
    }
}
