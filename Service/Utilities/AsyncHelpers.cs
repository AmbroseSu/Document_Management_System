namespace Service.Utilities;

public static class AsyncHelpers
{
    public static async Task<List<T>> WhereAsync<T>(this IEnumerable<T> source, Func<T, Task<bool>> predicate)
    {
        var results = await Task.WhenAll(source.Select(async item => (Item: item, IsMatch: await predicate(item))));
        return results.Where(x => x.IsMatch).Select(x => x.Item).ToList();
    }
}
