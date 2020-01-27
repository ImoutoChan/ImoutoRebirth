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

        public async Task<Tag> GetOrCreate(string name, Guid typeId, bool hasValue = false, string[]? synonyms = null)
            => await Get(name, typeId, hasValue, synonyms)
               ?? await Create(name, typeId, hasValue, synonyms);

        public async Task<Tag?> Get(string name, Guid typeId, bool hasValue, string[]? synonyms)
        {
            var result = await _lilinDbContext.Tags
                .Include(x => x.Type)
                .SingleOrDefaultAsync(x => x.Name == name && x.TypeId == typeId);

            if (result == null)
                return null;

            UpdateValues(result, hasValue, synonyms);
              
            return result.ToModel();
        }
        
        public async Task<Tag> Create(string name, Guid typeId, bool hasValue, string[]? synonyms)
        {
            var tag = new TagEntity
            {
                Id = Guid.NewGuid(),
                Name = name,
                HasValue = hasValue,
                SynonymsArray = synonyms ?? Array.Empty<string>(),
                TypeId = typeId
            };

            await _lilinDbContext.Tags.AddAsync(tag);
            await _lilinDbContext.SaveChangesAsync();

            tag = await _lilinDbContext.Tags.Include(x => x.Type).FirstAsync();

            return tag.ToModel();
        }

        private static void UpdateValues(TagEntity tag, bool hasValue, string[]? synonyms)
        {
            if (!tag.HasValue && hasValue)
                tag.HasValue = true;

            var newSynonyms = synonyms ?? Array.Empty<string>();

            if (newSynonyms.Any(x => !tag.SynonymsArray.Contains(x)))
            {
                tag.SynonymsArray = tag.SynonymsArray.Union(newSynonyms).ToArray();
            }
        }
    }
}