using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.Core.Infrastructure
{
    public interface ITagRepository
    {
        Task<Tag> GetOrCreate(
            string name,
            Guid typeId,
            bool hasValue = false,
            IReadOnlyCollection<string>? synonyms = null);

        Task<Tag?> Get(string name, Guid typeId, bool hasValue, IReadOnlyCollection<string>? synonyms);

        Task<Tag> Create(string name, Guid typeId, bool hasValue, IReadOnlyCollection<string>? synonyms);

        Task<IReadOnlyCollection<Tag>> Search(string? requestSearchPattern, int requestLimit);
    }
}