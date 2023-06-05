namespace ImoutoViewer.ImoutoRebirth.Services.Collections;

public class Collection
{
    public Collection(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    public Guid Id { get; }

    public string Name { get; }
}