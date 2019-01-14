using System;

namespace ImoutoRebirth.Lilin.Core.Models
{
    public class TagSearchEntry
    {
        public Guid TagId { get; }

        public string Value { get; }

        public TagSearchScope TagSearchScope { get; }

        public TagSearchEntry(Guid tagId, string value, TagSearchScope tagSearchScope)
        {
            TagId = tagId;
            Value = value;
            TagSearchScope = tagSearchScope;
        }
    }
}