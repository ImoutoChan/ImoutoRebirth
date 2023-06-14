namespace ImoutoRebirth.Common;

public static class AsyncEnumerableExtensions
{
    public static async Task<IReadOnlyList<T>> ToReadOnlyListAsync<T>(this IAsyncEnumerable<T> asyncEnumerable)
    {
        var result = new List<T>();
        await foreach (var item in asyncEnumerable)
        {
            result.Add(item);
        }
        return result;
    }
}
