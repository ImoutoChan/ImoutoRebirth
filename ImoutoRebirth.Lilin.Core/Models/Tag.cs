using System;
using System.Collections.Generic;
using System.Linq;
using ImoutoRebirth.Common;

namespace ImoutoRebirth.Lilin.Core.Models
{
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

        public static Tag CreateNew(TagType type, string name, bool hasValue, IReadOnlyCollection<string>? synonyms)
        {
            synonyms ??= Array.Empty<string>();

            return new Tag(Guid.NewGuid(), type, name, hasValue, synonyms, 0);
        }

        public void UpdateHasValue(in bool newHasValue)
        {
            if (!HasValue && newHasValue)
                HasValue = true;
        }

        public void UpdateSynonyms(IReadOnlyCollection<string> newSynonyms)
        {
            Synonyms = Synonyms.Union(newSynonyms).ToArray();
        }
    }
}