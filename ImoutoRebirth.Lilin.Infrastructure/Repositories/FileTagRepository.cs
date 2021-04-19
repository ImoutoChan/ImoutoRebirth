using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using ImoutoRebirth.Lilin.Core.Infrastructure;
using ImoutoRebirth.Lilin.Core.Models;
using ImoutoRebirth.Lilin.Core.Models.FileInfoAggregate;
using ImoutoRebirth.Lilin.DataAccess;
using ImoutoRebirth.Lilin.DataAccess.Entities;
using ImoutoRebirth.Lilin.Infrastructure.ExpressionHelpers;
using ImoutoRebirth.Lilin.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SQLitePCL;

namespace ImoutoRebirth.Lilin.Infrastructure.Repositories
{
    public class FileTagRepository : IFileTagRepository
    {
        private readonly LilinDbContext _lilinDbContext;
        private readonly ILogger<FileTagRepository> _logger;

        public FileTagRepository(LilinDbContext lilinDbContext, ILogger<FileTagRepository> logger)
        {
            _lilinDbContext = lilinDbContext;
            _logger = logger;
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

        public async Task Add(FileTag fileTag)
        {
            var entity = fileTag.ToEntity();
            await _lilinDbContext.FileTags.AddAsync(entity);

            await _lilinDbContext.SaveChangesAsync();
        }

        public async Task AddBatch(IReadOnlyCollection<FileTagInfo> fileTags)
        {
            var entities = fileTags.Select(x => x.ToEntity());
            await _lilinDbContext.AddRangeAsync(entities);

            await _lilinDbContext.SaveChangesAsync();
        }

        public async Task Delete(FileTag fileTag)
        {
            var query = GetByFileTag(fileTag);
            var (sql, _) = query.ToParametrizedSql();
            var (sql1, sql2) = BatchUtil.SplitLeadingCommentsAndMainSqlQuery(sql);
            _logger.LogInformation("Generated SQL: {Sql} {Sql1} {Sql2}", sql, sql1, sql2);
            await query.BatchDeleteAsync();
        }

        private IQueryable<FileTagEntity> GetByFileTag(FileTag fileTag)
        {
            return _lilinDbContext.FileTags.Where(
                x => x.Source == fileTag.Source
                     && x.TagId == fileTag.Tag.Id
                     && x.FileId == fileTag.FileId
                     && x.Value == fileTag.Value);
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
                    Expression<Func<FileTagEntity, bool>> predicateExpression;

                    // for cases where we should check tag values
                    if (!string.IsNullOrEmpty(f.Value))
                    {
                        // retrieve whenever we should check tag value for equality or inequality
                        var (checkEquals, value) = ExtractEqualityFlag(f.Value);

                        // retrieve whenever we should search for given value with * pattern
                        var (asteriskPlace, extractedValue) = ExtractAsteriskFlag(value);
                        value = extractedValue;

                        predicateExpression = (checkEquals, asteriskPlace) switch
                        {
                            (true, AsteriskPlace.None) => t
                                => t.TagId == f.TagId && t.Value == value,

                            (true, AsteriskPlace.Start) => t
                                => t.TagId == f.TagId && t.Value != null && t.Value.EndsWith(value),

                            (true, AsteriskPlace.End) => t
                                => t.TagId == f.TagId && t.Value != null && t.Value.StartsWith(value),

                            (true, AsteriskPlace.Both) => t
                                => t.TagId == f.TagId && t.Value != null && t.Value.Contains(value),

                            (false, AsteriskPlace.None) => t
                                => t.TagId == f.TagId && t.Value != value,

                            (false, AsteriskPlace.Start) => t
                                => t.TagId == f.TagId && (t.Value == null || !t.Value.EndsWith(value)),

                            (false, AsteriskPlace.End) => t
                                => t.TagId == f.TagId && (t.Value == null || !t.Value.StartsWith(value)),

                            (false, AsteriskPlace.Both) => t
                                => t.TagId == f.TagId && (t.Value == null || !t.Value.Contains(value)),

                            _ => throw new NotImplementedException("unsupported pattern scenario")
                        };
                    }

                    // for cases without values
                    else
                    {
                        predicateExpression = t => t.TagId == f.TagId;
                    }

                    condition = condition != null
                        ? condition.Or(predicateExpression)
                        : predicateExpression.Get();
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
        private static (AsteriskPlace Place, string Value) ExtractAsteriskFlag(string source)
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
        private static (bool flag, string value) ExtractEqualityFlag(string source)
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
