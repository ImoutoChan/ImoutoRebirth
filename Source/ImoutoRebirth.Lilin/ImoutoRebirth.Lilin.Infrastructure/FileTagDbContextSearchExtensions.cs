using System.Linq.Expressions;
using ImoutoRebirth.Lilin.Application.FileInfoSlice.Queries;
using ImoutoRebirth.Lilin.DataAccess;
using ImoutoRebirth.Lilin.DataAccess.Entities;
using ImoutoRebirth.Lilin.Infrastructure.ExpressionHelpers;

namespace ImoutoRebirth.Lilin.Infrastructure;

internal static class FileTagDbContextSearchExtensions
{
    public static IQueryable<FileTagEntity> GetSearchFilesQueryable(
        this LilinDbContext context,
        IReadOnlyCollection<TagSearchEntry> tagSearchEntries)
    {
        var fileTags = context.FileTags;
        IQueryable<FileTagEntity> files = context.FileTags;

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
            var includeFilter = MakeOrFilter(fileTags, new[] { tagSearchEntry });
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

        if (condition != null) query = query.Where(condition);

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
