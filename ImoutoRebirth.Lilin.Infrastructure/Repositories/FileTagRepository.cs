using System.Linq.Expressions;
using ImoutoRebirth.Lilin.Core.Infrastructure;
using ImoutoRebirth.Lilin.Core.Models;
using ImoutoRebirth.Lilin.Core.Models.FileInfoAggregate;
using ImoutoRebirth.Lilin.DataAccess;
using ImoutoRebirth.Lilin.DataAccess.Entities;
using ImoutoRebirth.Lilin.Infrastructure.ExpressionHelpers;
using ImoutoRebirth.Lilin.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NinjaNye.SearchExtensions;

namespace ImoutoRebirth.Lilin.Infrastructure.Repositories;

public class FileTagRepository : IFileTagRepository
{
    private readonly LilinDbContext _lilinDbContext;
    private readonly ILogger<FileTagRepository> _logger;

    public FileTagRepository(LilinDbContext lilinDbContext, ILogger<FileTagRepository> logger)
    {
        _lilinDbContext = lilinDbContext;
        _logger = logger;
    }

    public async Task<List<(string x, RelativeType?)>> SearchHashesInTags(IReadOnlyCollection<string> hashes)
    {
        var fileTags = await _lilinDbContext.FileTags
            .Search(x => x.Value).Containing(hashes.ToArray())
            .Include(x => x.Tag)
            .ToListAsync();
        
        return hashes.Select(x =>
        {
            var tags = fileTags.Where(y => y.Value?.Contains(x) == true).ToList();
            
            if (tags.Any(x => x.Tag?.Name == "ParentMd5"))
                return (x, RelativeType.Parent);
            
            if (tags.Any(x => x.Tag?.Name == "Child"))
                return (x, RelativeType.Child);
            
            return (x, (RelativeType?)null);
        }).ToList();
    }
    
    public async Task<Guid[]> SearchFiles(
        IReadOnlyCollection<TagSearchEntry> tagSearchEntries,
        int? limit = 100,
        int offset = 0)
    {
        var filteredFiles = GetSearchFilesQueryable(tagSearchEntries)
            .GroupBy(x => x.FileId)
            .Select(x => new { FileId = x.Key, FirstAppeared = x.Min(y => y.AddedOn) })
            .Distinct()
            .OrderBy(x => x.FirstAppeared);

        var filteredFileIds = filteredFiles.Select(x => x.FileId);

        filteredFileIds = filteredFileIds.Skip(offset);

        if (limit.HasValue)
            filteredFileIds = filteredFileIds.Take(limit.Value);

        return await filteredFileIds.ToArrayAsync();
    }

    public Task<int> SearchFilesCount(IReadOnlyCollection<TagSearchEntry> tagSearchEntries)
    {
        return GetSearchFilesQueryable(tagSearchEntries)
            .GroupBy(x => x.FileId)
            .Select(x => x.Key)
            .Distinct()
            .CountAsync();
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
        var tagsToDelete = await _lilinDbContext.FileTags.Where(
                x => x.Source == fileTag.Source
                     && x.TagId == fileTag.Tag.Id
                     && x.FileId == fileTag.FileId
                     && x.Value == fileTag.Value)
            .ToListAsync();

        _lilinDbContext.FileTags.RemoveRange(tagsToDelete);
        await _lilinDbContext.SaveChangesAsync();
    }

    private IQueryable<FileTagEntity> GetSearchFilesQueryable(IReadOnlyCollection<TagSearchEntry> tagSearchEntries)
    {
        var fileTags = _lilinDbContext.FileTags;
        IQueryable<FileTagEntity> files = _lilinDbContext.FileTags;

        // exclude tags
        var excludeFilters = tagSearchEntries
            .Where(x => x.TagSearchScope == TagSearchScope.Excluded)
            .ToArray();

        if (excludeFilters.Any())
        {
            var excludeFilter = MakeOrFilter(fileTags, excludeFilters);

            files = files.Where(f => !excludeFilter.Contains(f.FileId));
        }

        // include tags
        foreach (var tagSearchEntry in tagSearchEntries.Where(x => x.TagSearchScope == TagSearchScope.Included))
        {
            var includeFilter = MakeOrFilter(fileTags, new[] {tagSearchEntry});
            files = files.Where(f => includeFilter.Contains(f.FileId));
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
