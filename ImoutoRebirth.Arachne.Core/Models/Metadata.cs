using System;
using System.Collections.Generic;
using ImoutoRebirth.Common;

namespace ImoutoRebirth.Arachne.Core.Models
{
    public class Metadata : SearchResult
    {
        public bool IsFound { get; }

        public IReadOnlyCollection<Tag> Tags { get; }

        public IReadOnlyCollection<Note> Notes { get; }

        public int? FileIdFromSource { get; }

        public Metadata(
            Image image,
            SearchEngineType source,
            bool isFound,
            IReadOnlyCollection<Tag> tags,
            IReadOnlyCollection<Note> notes,
            int? fileIdFromSource) 
            : base(image, source)
        {
            ArgumentValidator.NotNull(() => image, () => tags, () => notes);

            IsFound = isFound;
            Tags = tags;
            Notes = notes;
            FileIdFromSource = fileIdFromSource;
        }

        public static Metadata NotFound(Image image, SearchEngineType source)
            => new Metadata(image, source, false, Array.Empty<Tag>(), Array.Empty<Note>(), null);
    }
}