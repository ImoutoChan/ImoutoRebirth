using System;
using System.Linq;
using System.Threading.Tasks;
using ImoutoRebirth.Lilin.Core.Infrastructure;
using ImoutoRebirth.Lilin.Core.Models;
using ImoutoRebirth.Lilin.DataAccess;
using ImoutoRebirth.Lilin.DataAccess.Entities;
using ImoutoRebirth.Lilin.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Lilin.Infrastructure.Repositories
{
    public class TagRepository : ITagRepository
    {
        private readonly LilinDbContext _lilinDbContext;

        public TagRepository(LilinDbContext lilinDbContext)
        {
            _lilinDbContext = lilinDbContext;
        }

        public async Task<Tag> GetOrCreate(string name, Guid typeId, bool hasValue = false, string[] synonyms = null)
            => await Get(name, typeId, hasValue, synonyms)
               ?? await Create(name, typeId, hasValue, synonyms);

        public async Task<Tag> Get(string name, Guid typeId, bool hasValue, string[] synonyms)
        {
            var result = await _lilinDbContext.Tags.SingleOrDefaultAsync(x => x.Name == name && x.TypeId == typeId);

            if (result == null)
                return null;

            await UpdateValues(result, hasValue, synonyms);
              
            return result.ToModel();
        }
        
        public async Task<Tag> Create(string name, Guid typeId, bool hasValue, string[] synonyms)
        {
            var tag = new TagEntity
            {
                Id = Guid.NewGuid(),
                Name = name,
                HasValue = hasValue,
                SynonymsArray = synonyms,
                TypeId = typeId
            };

            await _lilinDbContext.Tags.AddAsync(tag);
            await _lilinDbContext.SaveChangesAsync();

            return tag.ToModel();
        }

        public async Task IncrementTagCounter(Guid tagId)
        {
            var tag = await _lilinDbContext.Tags.FindAsync(tagId);

            tag.Count++;

            await _lilinDbContext.SaveChangesAsync();
        }

        private Task UpdateValues(TagEntity tag, bool hasValue, string[] synonyms)
        {
            if (!tag.HasValue && hasValue)
                tag.HasValue = true;

            if (tag.SynonymsArray.Any(x => !tag.SynonymsArray.Contains(x)))
                tag.SynonymsArray = tag.SynonymsArray.Union(synonyms).ToArray();

            return _lilinDbContext.SaveChangesAsync();
        }
    }
}