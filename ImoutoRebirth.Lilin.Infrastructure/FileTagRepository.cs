using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ImoutoRebirth.Lilin.Core.Infrastructure;
using ImoutoRebirth.Lilin.Core.Models;
using ImoutoRebirth.Lilin.DataAccess;
using ImoutoRebirth.Lilin.DataAccess.Entities;
using ImoutoRebirth.Lilin.Infrastructure.ExpressionHelpers;
using ImoutoRebirth.Lilin.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Lilin.Infrastructure
{
    public class FileTagRepository : IFileTagRepository
    {
        private readonly LilinDbContext _lilinDbContext;

        public FileTagRepository(LilinDbContext lilinDbContext)
        {
            _lilinDbContext = lilinDbContext;
        }

        public IUnitOfWork UnitOfWork => _lilinDbContext;
        
        public async Task Add(FileTag fileTag)
        {
            var tag = await _lilinDbContext.Tags.FirstAsync(x => x.Id == fileTag.Tag.Id);
            await _lilinDbContext.FileTags.AddAsync(fileTag.ToEntity());
            tag.Count++;
        }

        public async Task<Guid[]> SearchFiles(
            IReadOnlyCollection<TagSearchEntry> tagSearchEntries,
            uint? limit = 100,
            uint offset = 0)
        {
            var files = GetSearchFilesQueryable(tagSearchEntries);

            files = files.Skip((int)offset);

            if (limit.HasValue)
                files = files.Take((int)limit.Value);

            return await files.ToArrayAsync();
        }

        public async Task<uint> SearchFilesCount(IReadOnlyCollection<TagSearchEntry> tagSearchEntries)
        {
            var files = GetSearchFilesQueryable(tagSearchEntries);
            return (uint)await files.CountAsync();
        }

        public async Task<IReadOnlyCollection<FileTag>> GetForFile(Guid fileId)
        {
            var results = await _lilinDbContext.FileTags
                                               .Include(x => x.Tag)
                                               .ThenInclude(x => x.Type)
                                               .Where(x => x.FileId == fileId)
                                               .AsNoTracking()
                                               .ToArrayAsync();

            return results.Select(x => x.ToModel()).ToArray();
        }

        private IQueryable<Guid> GetSearchFilesQueryable(IReadOnlyCollection<TagSearchEntry> tagSearchEntries)
        {
            var fileTags = _lilinDbContext.FileTags;
            var files = _lilinDbContext.FileTags.Select(x => x.FileId).Distinct();

            // exclude tags
            var excludeFilters = tagSearchEntries
                                .Where(x => x.TagSearchScope == TagSearchScope.Excluded)
                                .ToArray();

            if (excludeFilters.Any())
            {
                var excludeFilter = MakeOrFilter(fileTags, excludeFilters);

                files = files.Where(f => !excludeFilter.Contains(f));
            }

            // include tags
            foreach (var tagSearchEntry in tagSearchEntries.Where(x => x.TagSearchScope == TagSearchScope.Included))
            {
                var includeFilter = MakeOrFilter(fileTags, new[] {tagSearchEntry});
                files = files.Where(f => includeFilter.Contains(f));
            }

            return files;
        }

        private static IQueryable<Guid> MakeOrFilter(
            IQueryable<FileTagEntity> fileTags, 
            IReadOnlyCollection<TagSearchEntry> filters)
        {
            var query = fileTags;

            Expression<Func<FileTagEntity, bool>> condition = null;
            if (filters.Any())
            {
                foreach (var f in filters)
                {
                    Expression<Func<FileTagEntity, bool>> fcond;

                    if (!String.IsNullOrEmpty(f.Value))
                    {
                        var (checkEqual, value) = ExtractValue(f.Value);

                        if (checkEqual)
                        {
                            fcond = t => t.TagId == f.TagId && t.Value == value;
                        }
                        else
                        {
                            fcond = t => t.TagId == f.TagId && t.Value != value;
                        }
                    }
                    else
                    {
                        fcond = t => t.TagId == f.TagId;
                    }

                    if (condition == null)
                    {

                        condition = PredicateBuilder.Get(fcond);
                    }
                    else
                    {
                        condition = condition.Or(fcond);
                    }
                }
            }

            if (condition != null)
            {
                query = query.Where(condition);
            }

            return query.Select(x => x.FileId);
        }

        private static (bool flag, string value) ExtractValue(string source)
        {
            var flag = source.Substring(0, 1);
            if (flag == "=")
            {
                return (true, source.Substring(1));
            }
            else // == '!='
            {
                return (false, source.Substring(2));
            }
        }
    }
}