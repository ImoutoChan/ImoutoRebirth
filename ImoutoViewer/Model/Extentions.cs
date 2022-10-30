using GihanSoft.String;

namespace ImoutoViewer.Model;

internal static class Extentions
{
    public static IOrderedEnumerable<TSource> OrderByWithDirection<TSource, TKey>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        bool descending)
    {
        // if (typeof(TSource) == typeof(string))
        // {
        //     return descending
        //         ? source.OrderByDescending(
        //             x => (string)(object)keySelector(x)!,
        //             NaturalComparer.InvariantCultureIgnoreCase)
        //         : source.OrderBy(
        //             x => (string)(object)keySelector(x)!, 
        //             NaturalComparer.InvariantCultureIgnoreCase);
        // }
        
        return descending
            ? source.OrderByDescending(keySelector)
            : source.OrderBy(keySelector);
    }
}
