using System;
using System.Collections.Generic;

namespace GTA_SA_PathsRedactor.Services.Extensions;

public static class IEnumerableExtensions
{
    public static void ForEach<TSource>(this IEnumerable<TSource>? source, Action<TSource> action)
    {
        if (source is null)
            return;
        
        foreach (var item in source)
        {
            action(item);
        }
    }

    public static int IndexOf<TSource>(this IEnumerable<TSource>? source, TSource element)
    {
        if (source is null)
            return -1;

        var idx = 0;
        foreach (var item in source)
        {
            if (item!.Equals(element))
                return idx;

            idx++;
        }

        return -1;
    }
    
    public static int IndexOf<TSource>(this IEnumerable<TSource>? source, Predicate<TSource> predicate)
    {
        if (source is null)
            return -1;

        var idx = 0;
        foreach (var item in source)
        {
            if (predicate(item))
                return idx;

            idx++;
        }

        return -1;
    }
}