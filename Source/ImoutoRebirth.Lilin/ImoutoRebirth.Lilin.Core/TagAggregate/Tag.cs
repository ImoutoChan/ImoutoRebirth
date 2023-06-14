using ImoutoRebirth.Common;
using ImoutoRebirth.Lilin.Core.TagTypeAggregate;

namespace ImoutoRebirth.Lilin.Core.TagAggregate;

public class Tag
{
    public Guid Id { get; }

    public TagType Type { get; }

    public string Name { get; }

    public bool HasValue { get; private set; }

    public IReadOnlyCollection<string> Synonyms { get; private set; }

    public int Count { get; }

    public Tag(Guid id, TagType type, string name, bool hasValue, IReadOnlyCollection<string> synonyms, int count)
    {
        ArgumentValidator.NotNull(() => type, () => name);
        ArgumentValidator.Requires(() => !string.IsNullOrWhiteSpace(name), nameof(name));

        Id = id;
        Type = type;
        Name = name;
        HasValue = hasValue;
        Synonyms = synonyms;
        Count = count;
    }

    public static Tag Create(TagType type, string name, bool hasValue, IReadOnlyCollection<string>? synonyms)
    {
        synonyms ??= Array.Empty<string>();

        return new Tag(Guid.NewGuid(), type, name, hasValue, synonyms, 0);
    }

    public void UpdateHasValue(bool hasValue)
    {
        HasValue = hasValue;
    }

    public void UpdateSynonyms(IEnumerable<string> synonyms)
    {
        Synonyms = Synonyms.Union(synonyms).ToList();
    }
}
