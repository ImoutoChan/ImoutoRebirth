using System;
using System.Collections.Generic;
using ImoutoRebirth.Common;

namespace ImoutoRebirth.Lilin.Core.Models
{
    public class Tag
    {
        public Guid Id { get; }

        public TagType Type { get; }

        public string Name { get; }

        public bool HasValue { get; }

        public IReadOnlyCollection<string> Synonyms { get; }

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
    }
}