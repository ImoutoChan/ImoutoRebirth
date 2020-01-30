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

        public async Task<Tag?> Get(string name, Guid typeId)
        {
            var result = await _lilinDbContext.Tags
                .Include(x => x.Type)
                .SingleOrDefaultAsync(x => x.Name == name && x.TypeId == typeId);

            return result?.ToModel();
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

        public async Task Update(Tag tag)
        {
            var loadedTag = await _lilinDbContext.Tags.SingleAsync(x => x.Id == tag.Id);

            loadedTag.HasValue = tag.HasValue;
            loadedTag.SynonymsArray = tag.Synonyms;
        }

        public async Task Create(Tag tag)
        {
            var newEntity = new TagEntity
            {
                Id = Guid.NewGuid(),
                Name = tag.Name,
                HasValue = tag.HasValue,
                SynonymsArray = tag.Synonyms,
                TypeId = tag.Type.Id
            };

            await _lilinDbContext.Tags.AddAsync(newEntity);
        }
    }
}