using System;
using System.Collections.Generic;
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

        public async Task<Tag> GetOrCreate(
            string name, 
            Guid typeId, 
            bool hasValue = false, 
            IReadOnlyCollection<string>? synonyms = null)
            => await Get(name, typeId, hasValue, synonyms)
               ?? await Create(name, typeId, hasValue, synonyms);

        public async Task<Tag?> Get(string name, Guid typeId, bool hasValue, IReadOnlyCollection<string>? synonyms)
        {
            var result = await _lilinDbContext.Tags
                .Include(x => x.Type)
                .SingleOrDefaultAsync(x => x.Name == name && x.TypeId == typeId);

            if (result == null)
                return null;

            UpdateValues(result, hasValue, synonyms);
              
            return result.ToModel();
        }
        
        public async Task<Tag> Create(string name, Guid typeId, bool hasValue, IReadOnlyCollection<string>? synonyms)
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

            tag = await _lilinDbContext.Tags
                .Include(x => x.Type)
                .FirstAsync(x => x.Id == tag.Id);

            return tag.ToModel();
        }

        public async Task<IReadOnlyCollection<Tag>> Search(string? requestSearchPattern, int requestLimit)
        {
            var tagsWithTypes = _lilinDbContext.Tags
                .OrderByDescending(x => x.Count)
                .Include(x => x.Type);
            
            List<TagEntity> finalResult;
            if (string.IsNullOrEmpty(requestSearchPattern))
            {
                finalResult = await tagsWithTypes.Take(requestLimit).ToListAsync();
            }
            else
            {
                finalResult = await tagsWithTypes
                    .Where(x => x.Name.StartsWith(requestSearchPattern))
                    .Take(requestLimit)
                    .ToListAsync();

                if (finalResult.Count < requestLimit)
                {
                    requestLimit -= finalResult.Count;

                    var contains = await tagsWithTypes
                        .Where(x => !x.Name.StartsWith(requestSearchPattern))
                        .Where(x => x.Name.Contains(requestSearchPattern))
                        .Take(requestLimit)
                        .ToListAsync();

                    finalResult.AddRange(contains);
                }
            }
            
            return finalResult.Select(x => x.ToModel()).ToArray();
        }

        private static void UpdateValues(TagEntity tag, bool hasValue, IReadOnlyCollection<string>? synonyms)
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