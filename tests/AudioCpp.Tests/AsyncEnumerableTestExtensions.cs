using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mabs.AudioCpp.Tests;

/// <summary>Drains async enumerable into lists for assertions.</summary>
internal static class AsyncEnumerableTestExtensions
{
    /// <summary>Collects every element yielded by the source.</summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The async enumerable to drain.</param>
    /// <returns>A list of all elements in enumeration order.</returns>
    internal static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source)
    {
        List<T> items = [];
        await foreach (T item in source)
        {
            items.Add(item);
        }

        return items;
    }
}