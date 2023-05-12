namespace ImoutoRebirth.Room.WebApi.Responses;

public class CollectionResponse
{
    public Guid Id { get; }

    public string Name { get; }

    public CollectionResponse(Guid id, string name)
    {
        Id = id;
        Name = name;
    }
}