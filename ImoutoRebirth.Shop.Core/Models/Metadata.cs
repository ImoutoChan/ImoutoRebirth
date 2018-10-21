using System;
using System.Collections.Generic;
using ImoutoRebirth.Common;

namespace ImoutoRebirth.Shop.Core.Models
{
    public class Metadata : SearchResult
    {
        public bool IsFound { get; }

        public IReadOnlyCollection<Tag> Tags { get; }

        public IReadOnlyCollection<Note> Notes { get; }

        public Metadata(
            Image image,
            SearchEngineType source,
            bool isFound,
            IReadOnlyCollection<Tag> tags,
            IReadOnlyCollection<Note> notes) 
            : base(image, source)
        {
            ArgumentValidator.NotNull(() => image, () => tags, () => notes);

            IsFound = isFound;
            Tags = tags;
            Notes = notes;
        }

        public static Metadata NotFound(Image image, SearchEngineType source)
            => new Metadata(image, source, false, Array.Empty<Tag>(), Array.Empty<Note>());
    }
}