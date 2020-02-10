using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ImoutoRebirth.Lilin.Core.Infrastructure;
using ImoutoRebirth.Lilin.Core.Models;
using ImoutoRebirth.Lilin.Core.Models.FileInfoAggregate;
using ImoutoRebirth.Lilin.DataAccess;
using ImoutoRebirth.Lilin.DataAccess.Entities;
using ImoutoRebirth.Lilin.Infrastructure.ExpressionHelpers;
using ImoutoRebirth.Lilin.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Lilin.Infrastructure.Repositories
{
    public class FileTagRepository : IFileTagRepository
    {
        private readonly LilinDbContext _lilinDbContext;

        public FileTagRepository(LilinDbContext lilinDbContext)
        {
            _lilinDbContext = lilinDbContext;
        }

        public async Task<Guid[]> SearchFiles(
            IReadOnlyCollection<TagSearchEntry> tagSearchEntries,
            int? limit = 100,
            int offset = 0)
        {
            var files = GetSearchFilesQueryable(tagSearchEntries);

            // workaround for https://github.com/dotnet/efcore/issues/8523
            files = files.OrderBy(x => x);

            files = files.Skip(offset);

            if (limit.HasValue)
                files = files.Take(limit.Value);

            return await files.ToArrayAsync();
        }

        public async Task<int> SearchFilesCount(IReadOnlyCollection<TagSearchEntry> tagSearchEntries)
        {
            var files = GetSearchFilesQueryable(tagSearchEntries);
            return await files.CountAsync();
        }

        public async Task<IReadOnlyCollection<FileTag>> GetForFile(Guid fileId)
        {
            var results = await _lilinDbContext.FileTags
                                               .Include(x => x.Tag)
                                               .ThenInclude(x => x!.Type)
                                               .Where(x => x.FileId == fileId)
                                               .AsNoTracking()
                                               .ToArrayAsync();

            return results.Select(x => x.ToModel()).ToArray();
        }
        
        public async Task Update(FileTag fileTag)
        {
            var fileTagEntity = await GetByFileTag(fileTag).SingleAsync();

            fileTagEntity.Value = fileTag.Value;

            await _lilinDbContext.SaveChangesAsync();
        }

        public async Task Add(FileTag fileTag)
        {
            var entity = fileTag.ToEntity();
            await _lilinDbContext.FileTags.AddAsync(entity);

            await _lilinDbContext.SaveChangesAsync();
        }

        public async Task Delete(FileTag fileTag)
        {
            var fileTagEntity = await GetByFileTag(fileTag).SingleAsync();

            _lilinDbContext.FileTags.Remove(fileTagEntity);

            await _lilinDbContext.SaveChangesAsync();
        }

        private IQueryable<FileTagEntity> GetByFileTag(FileTag fileTag)
        {
            return _lilinDbContext.FileTags.Where(
                x => x.Source == fileTag.Source
                     && x.TagId == fileTag.Tag.Id
                     && x.FileId == fileTag.FileId);
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

            Expression<Func<FileTagEntity, bool>>? condition = null;
            if (filters.Any())
            {
                foreach (var f in filters)
                {
                    Expression<Func<FileTagEntity, bool>> fcond;

                    if (!String.IsNullOrEmpty(f.Value))
                    {
                        var (checkEqual, value) = ExtractValue(f.Value);
                        var asteriskValue = ExtractAsteriskValue(value);
                        value = asteriskValue.Value;

                        switch (checkEqual, asteriskValue.Place)
                        {
                            case (true, AsteriskPlace.None):
                                fcond = t => t.TagId == f.TagId && t.Value == value;
                                break;
                            case (true, AsteriskPlace.Start):
                                fcond = t => t.TagId == f.TagId && t.Value != null && t.Value.EndsWith(value);
                                break;
                            case (true, AsteriskPlace.End):
                                fcond = t => t.TagId == f.TagId && t.Value != null && t.Value.StartsWith(value);
                                break;
                            case (true, AsteriskPlace.Both):
                                fcond = t => t.TagId == f.TagId && t.Value != null && t.Value.Contains(value);
                                break;

                            case (false, AsteriskPlace.None):
                                fcond = t => t.TagId == f.TagId && t.Value != value;
                                break;
                            case (false, AsteriskPlace.Start):
                                fcond = t => t.TagId == f.TagId && (t.Value == null || !t.Value.EndsWith(value));
                                break;
                            case (false, AsteriskPlace.End):
                                fcond = t => t.TagId == f.TagId && (t.Value == null || !t.Value.StartsWith(value));
                                break;
                            case (false, AsteriskPlace.Both):
                                fcond = t => t.TagId == f.TagId && (t.Value == null || !t.Value.Contains(value));
                                break;

                            default:
                                throw new NotImplementedException("unsupported pattern scenario");
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

        /// <summary>
        ///     Convert string with asterisk symbol to tuple:
        ///     '*xxx' => start, 'xxx'
        ///     'xxx' => none, 'xxx'
        ///     'xxx*' => end, 'xxx'
        /// </summary>
        private static (AsteriskPlace Place, string Value) ExtractAsteriskValue(string source)
            => (source[0], source[^1]) switch
            {
                ('*', '*') => (AsteriskPlace.Both, source[1..^2]),
                ('*', _) => (AsteriskPlace.Start, source[1..]),
                (_, '*') => (AsteriskPlace.End, source[..^2]),
                _ => (AsteriskPlace.None, source)
            };
        
        /// <summary>
        ///     Convert string with equality symbol to tuple:
        ///     'xxx' => true, 'xxx'
        ///     '!=xxx' => false, 'xxx'
        ///     '!asd' => false, 'asd'
        ///     '=asd' => true, 'asd'
        /// </summary>
        private static (bool flag, string value) ExtractValue(string source)
            => (source[0], source[1]) switch
            {
                ('=', _) => (true, source.Substring(1)),
                ('!', '=') => (false, source.Substring(2)),
                ('!', _) => (false, source.Substring(1)),
                _ => (true, source)
            };

        private enum AsteriskPlace
        {
            None,
            Start,
            End,
            Both
        }
    }
}