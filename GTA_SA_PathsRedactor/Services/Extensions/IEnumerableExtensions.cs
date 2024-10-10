using System;
using System.Collections.Generic;

namespace GTA_SA_PathsRedactor.Services.Extensions;

public static class IEnumerableExtensions
{
    public static void ForEach<TSource>(this IEnumerable<TSource>? source, Action<TSource> action)
    {
        if (source is null)
        {
            return;
        }
        
        foreach (var item in source)
        {
            action(item);
        }
    }
}