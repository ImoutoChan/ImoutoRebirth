using ImoutoRebirth.Common;
using ImoutoRebirth.Lilin.Domain.TagTypeAggregate;

namespace ImoutoRebirth.Lilin.Domain.TagAggregate;

public class Tag
{
    public Guid Id { get; }

    public TagType Type { get; }

    public string Name { get; }

    public bool HasValue { get; private set; }

    public IReadOnlyCollection<string> Synonyms { get; private set; }
    
    public TagOptions Options { get; private set; }

    public int Count { get; }

    public Tag(
        Guid id,
        TagType type,
        string name,
        bool hasValue,
        IReadOnlyCollection<string> synonyms,
        TagOptions options,
        int count)
    {
        ArgumentValidator.NotNull(() => type, () => name);
        ArgumentValidator.Requires(() => !string.IsNullOrWhiteSpace(name), nameof(name));

        Id = id;
        Type = type;
        Name = name;
        HasValue = hasValue;
        Synonyms = synonyms;
        Options = options;
        Count = count;
    }

    public static Tag Create(
        TagType type,
        string name,
        bool hasValue,
        IReadOnlyCollection<string>? synonyms,
        TagOptions options)
    {
        synonyms ??= Array.Empty<string>();

        return new Tag(Guid.NewGuid(), type, name, hasValue, synonyms, options, 0);
    }

    public void UpdateHasValue(bool hasValue)
    {
        HasValue = hasValue;
    }

    public void UpdateSynonyms(IEnumerable<string> synonyms)
    {
        Synonyms = Synonyms.Union(synonyms).ToList();
    }

    public void UpdateOptions(TagOptions options)
    {
        Options = options;
    }
}

[Flags]
public enum TagOptions
{
    None = 0,
    Counter = 1
}
