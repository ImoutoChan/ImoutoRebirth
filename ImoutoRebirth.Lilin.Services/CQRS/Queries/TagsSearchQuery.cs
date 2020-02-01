using System.Collections.Generic;
using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.Services.CQRS.Queries
{
    public class TagsSearchQuery : IQuery<IReadOnlyCollection<Tag>>
    {
        private const int DefaultLimit = 10;

        public string? SearchPattern { get; }

        public int Limit { get; }

        public TagsSearchQuery(string? searchPattern, int? limit)
        {
            SearchPattern = searchPattern;
            Limit = limit ?? DefaultLimit;
        }
    }
}