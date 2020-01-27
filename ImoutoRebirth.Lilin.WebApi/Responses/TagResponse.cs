using System;
using System.Collections.Generic;

namespace ImoutoRebirth.Lilin.WebApi.Responses
{
    public class TagResponse
    {
        public Guid Id { get; }

        public TagTypeResponse Type { get; }

        public string Name { get; }

        public bool HasValue { get; }

        public IReadOnlyCollection<string> Synonyms { get; }

        public int Count { get; }

        public TagResponse(
            Guid id, 
            TagTypeResponse type, 
            string name, 
            bool hasValue, 
            IReadOnlyCollection<string> synonyms, 
            int count)
        {

            Id = id;
            Type = type;
            Name = name;
            HasValue = hasValue;
            Synonyms = synonyms;
            Count = count;
        }
    }
}