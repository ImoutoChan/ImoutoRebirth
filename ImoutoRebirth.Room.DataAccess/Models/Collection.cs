using ImoutoRebirth.Room.DataAccess.Models.Abstract;

namespace ImoutoRebirth.Room.DataAccess.Models;

public class Collection : ModelBase
{
    public string Name { get; }

    public Collection(Guid id, string name) : base(id)
    {
        Name = name;
    }
}