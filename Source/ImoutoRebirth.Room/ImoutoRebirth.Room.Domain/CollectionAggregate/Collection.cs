using ImoutoRebirth.Room.Domain.CollectionAggregate;

namespace ImoutoRebirth.Room.Domain;

public class Collection
{
    private Collection(
        Guid id,
        string name,
        DestinationFolder destinationFolder,
        IReadOnlyCollection<SourceFolder> sourceFolders)
    {
        Id = id;
        Name = name;
        DestinationFolder = destinationFolder;
        SourceFolders = sourceFolders;
    }

    public Guid Id { get; }

    public string Name { get; }

    public DestinationFolder DestinationFolder { get; }

    public IReadOnlyCollection<SourceFolder> SourceFolders { get; }
}
