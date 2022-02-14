using System.Diagnostics;
using ImoutoRebirth.Common;

namespace ImoutoRebirth.Arachne.Core.Models;

[DebuggerDisplay("[{Type}] {Name} : {Value}")]
public class Tag
{
    public string Type { get; }

    public string Name { get; }

    public string? Value { get; }

    public IReadOnlyCollection<string> Synonyms { get; }

    public Tag(string type, string name, string? value = null, IReadOnlyCollection<string>? synonyms = null)
    {
        ArgumentValidator.NotNull(() => type, () => name);

        Type = type;
        Name = name;
        Value = value;
        Synonyms = synonyms ?? Array.Empty<string>();
    }
}