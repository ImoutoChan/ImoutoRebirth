using System.Linq.Expressions;
using ImoutoRebirth.Lilin.Application.FileInfoSlice.Queries;
using ImoutoRebirth.Lilin.DataAccess;
using ImoutoRebirth.Lilin.DataAccess.Entities;
using ImoutoRebirth.Lilin.Infrastructure.ExpressionHelpers;

namespace ImoutoRebirth.Lilin.Infrastructure;

internal enum SearchOptions
{
    FileShouldHaveAllIncludedTags,
    FileShouldHaveAnyIncludedTags
}

internal static class FileTagDbContextSearchExtensions
{
    public static IQueryable<Guid> GetSearchFilesIdsQueryable(
        this LilinDbContext context,
        IReadOnlyCollection<TagSearchEntry> tagSearchEntries,
        SearchOptions searchOptions = SearchOptions.FileShouldHaveAllIncludedTags)
    {
        IQueryable<Guid>? queryable;

        var include = tagSearchEntries.Where(x => x.TagSearchScope == TagSearchScope.Included).ToList();
        var exclude = tagSearchEntries.Where(x => x.TagSearchScope == TagSearchScope.Excluded).ToList();

        if (include.Any())
        {
            var includeFirst = include.First();

            queryable = context.FileTags
                .Where(x => x.TagId == includeFirst.TagId)
                .MakeValueFilter(includeFirst.Value)
                .Select(x => x.FileId);

            foreach (var includeEntry in include.Skip(1))
            {
                if (searchOptions == SearchOptions.FileShouldHaveAllIncludedTags)
                {
                    queryable = queryable.Intersect(
                        context.FileTags
                            .Where(x => x.TagId == includeEntry.TagId)
                            .MakeValueFilter(includeEntry.Value)
                            .Select(x => x.FileId)
                    );
                }
                else if (searchOptions == SearchOptions.FileShouldHaveAnyIncludedTags)
                {
                    queryable = queryable.Union(
                        context.FileTags
                            .Where(x => x.TagId == includeEntry.TagId)
                            .MakeValueFilter(includeEntry.Value)
                            .Select(x => x.FileId)
                    );
                }
            }
        }
        else
        {
            queryable = context.FileTags.Select(x => x.FileId);
        }

        foreach (var excludeEntry in exclude)
        {
            queryable = queryable.Except(
                context.FileTags
                    .Where(x => x.TagId == excludeEntry.TagId)
                    .MakeValueFilter(excludeEntry.Value)
                    .Select(x => x.FileId)
            );
        }

        return queryable;
    }

    public static IQueryable<FileTagEntity> MakeValueFilter(
        this IQueryable<FileTagEntity> query, 
        string? valueFilter)
    {
        if (string.IsNullOrWhiteSpace(valueFilter))
            return query;
        
        // retrieve whenever we should check tag value for equality or inequality
        var (checkEquals, value) = ExtractEqualityFlag(valueFilter);

        // retrieve whenever we should search for given value with * pattern
        var (asteriskPlace, extractedValue) = ExtractAsteriskFlag(value);
        value = extractedValue;

        Expression<Func<FileTagEntity, bool>> predicateExpression = (checkEquals, asteriskPlace) switch
        {
            (true, AsteriskPlace.None) => t
                => t.Value == value,

            (true, AsteriskPlace.Start) => t
                => t.Value != null && t.Value.EndsWith(value),

            (true, AsteriskPlace.End) => t
                => t.Value != null && t.Value.StartsWith(value),

            (true, AsteriskPlace.Both) => t
                => t.Value != null && t.Value.Contains(value),

            (false, AsteriskPlace.None) => t
                => t.Value != value,

            (false, AsteriskPlace.Start) => t
                => t.Value == null || !t.Value.EndsWith(value),

            (false, AsteriskPlace.End) => t
                => t.Value == null || !t.Value.StartsWith(value),

            (false, AsteriskPlace.Both) => t
                => t.Value == null || !t.Value.Contains(value),

            _ => throw new NotImplementedException("unsupported pattern scenario")
        };

        return query.Where(predicateExpression);
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
    private static (bool flag, string value) ExtractEqualityFlag(string tagValue)
        => (tagValue[0], tagValue[1]) switch
        {
            ('=', _) => (true, tagValue.Substring(1)),
            ('!', '=') => (false, tagValue.Substring(2)),
            ('!', _) => (false, tagValue.Substring(1)),
            _ => (true, tagValue)
        };

    private enum AsteriskPlace
    {
        None,
        Start,
        End,
        Both
    }
}
