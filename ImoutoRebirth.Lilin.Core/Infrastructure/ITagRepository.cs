using System;
using System.Threading.Tasks;
using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.Core.Infrastructure
{
    public interface ITagRepository
    {
        Task<Tag> GetOrCreate(string name, Guid typeId, bool hasValue = false, string[]? synonyms = null);
        Task<Tag?> Get(string name, Guid typeId, bool hasValue, string[]? synonyms);
        Task<Tag> Create(string name, Guid typeId, bool hasValue, string[]? synonyms);
    }
}