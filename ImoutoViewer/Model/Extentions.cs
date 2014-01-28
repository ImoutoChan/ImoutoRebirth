﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ImoutoViewer.Model
{
    public static class Extentions
    {
        public static IOrderedEnumerable<TSource> OrderByWithDirection<TSource, TKey>
                        (this IEnumerable<TSource> source,
                         Func<TSource, TKey> keySelector,
                         bool descending)
        {
            return descending ? source.OrderByDescending(keySelector)
                              : source.OrderBy(keySelector);
        }

        public static IOrderedQueryable<TSource> OrderByWithDirection<TSource, TKey>
                        (this IQueryable<TSource> source,
                         Expression<Func<TSource, TKey>> keySelector,
                         bool descending)
        {
            return descending ? source.OrderByDescending(keySelector)
                              : source.OrderBy(keySelector);
        }

    }
}
