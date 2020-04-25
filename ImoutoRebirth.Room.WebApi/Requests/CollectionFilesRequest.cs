using System;
using System.Collections.Generic;

namespace ImoutoRebirth.Room.WebApi.Requests
{
    public class CollectionFilesRequest
    {
        public Guid? CollectionId { get; set; }

        public IReadOnlyCollection<Guid> CollectionFileIds { get; set; }

        public string Path { get; set; }

        public IReadOnlyCollection<string> Md5 { get; set; }

        public int? Count { get; set; }

        [PresentedOnlyIfNotNull(nameof(Count))]
        public int? Skip { get; set; }
    }
}