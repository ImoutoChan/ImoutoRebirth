using System;
using System.Collections.Generic;

namespace ImoutoRebirth.Room.DataAccess.Repositories.Queries
{
    // ReSharper disable once ClassNeverInstantiated.Global
    // Automapper
    public class CollectionFilesQuery
    {
        public Guid? CollectionId { get; }

        public IReadOnlyCollection<Guid> CollectionFileIds { get; }

        public string Path { get; }

        public string Md5 { get; }

        public int? Count { get; }

        public int? Skip { get; }

        public CollectionFilesQuery(
            Guid? collectionId,
            IReadOnlyCollection<Guid> collectionFileIds,
            string path,
            string md5,
            int? count,
            int? skip)
        {
            CollectionId = collectionId;
            CollectionFileIds = collectionFileIds;
            Path = path;
            Md5 = md5?.ToLowerInvariant();
            Count = count;
            Skip = skip;
        }
    }
}