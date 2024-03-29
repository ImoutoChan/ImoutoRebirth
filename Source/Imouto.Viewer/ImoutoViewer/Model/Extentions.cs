﻿namespace ImoutoViewer.Model;

internal static class Extentions
{
    public static IOrderedEnumerable<TSource> OrderByWithDirection<TSource, TKey>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        bool descending)
    {
        return descending
            ? source.OrderByDescending(keySelector)
            : source.OrderBy(keySelector);
    }
}
