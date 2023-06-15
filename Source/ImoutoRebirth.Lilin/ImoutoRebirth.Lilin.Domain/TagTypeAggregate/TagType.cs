using ImoutoRebirth.Common;

namespace ImoutoRebirth.Lilin.Domain.TagTypeAggregate;

public class TagType
{
    public Guid Id { get; }

    public string Name { get; }

    public int Color { get; }

    public TagType(Guid id, string name, int color)
    {
        ArgumentValidator.NotNull(() => name);
        ArgumentValidator.Requires(() => !string.IsNullOrWhiteSpace(name), nameof(name));

        Id = id;
        Name = name;
        Color = color;
    }
}
