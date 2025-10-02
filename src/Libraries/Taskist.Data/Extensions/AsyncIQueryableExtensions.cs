using LinqToDB;
using Taskist.Core.Common;

namespace Taskist.Data.Extensions;

public static class AsyncIQueryableExtensions
{
    public static Task<List<TSource>> ToListAsync<TSource>(this IQueryable<TSource> source)
    {
        return AsyncExtensions.ToListAsync(source);
    }

    public static async Task<IPagedList<T>> ToPagedListAsync<T>(this IQueryable<T> source, int pageIndex, int pageSize, bool getOnlyTotalCount = false)
    {
        if (source == null)
            return new PagedList<T>(new List<T>(), pageIndex, pageSize);

        pageSize = Math.Max(pageSize, 1);

        var count = await source.CountAsync();

        var data = new List<T>();

        if (!getOnlyTotalCount)
            data.AddRange(await source.Skip(pageIndex).Take(pageSize).ToListAsync());

        return new PagedList<T>(data, pageIndex, pageSize, count);
    }
}
