using System.Collections.Generic;
using ImoutoRebirth.Common;

namespace ImoutoRebirth.Arachne.Core.Models
{
    public class Tag
    {
        public string Type { get; }

        public string Name { get; }

        public string Value { get; }

        public IReadOnlyCollection<string> Synonyms { get; }

        public Tag(string type, string name, string value, IReadOnlyCollection<string> synonyms)
        {
            ArgumentValidator.NotNull(() => type, () => name, () => value, () => synonyms);

            Type = type;
            Name = name;
            Value = value;
            Synonyms = synonyms;
        }
    }
}