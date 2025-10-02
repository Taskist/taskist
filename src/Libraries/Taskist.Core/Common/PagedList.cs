namespace Taskist.Core.Common;

[Serializable]
public class PagedList<T> : List<T>, IPagedList<T>
{
    public PagedList(IList<T> source, int pageIndex, int pageSize, int? totalCount = null)
    {
        pageSize = Math.Max(pageSize, 1);

        TotalCount = totalCount ?? source.Count;
        TotalPages = TotalCount / pageSize;

        if (TotalCount % pageSize > 0)
            TotalPages++;

        PageSize = pageSize;
        PageIndex = pageIndex;
        AddRange(totalCount != null ? source : source.Skip(pageIndex * pageSize).Take(pageSize));
    }

    public int PageIndex { get; }

    public int PageSize { get; }

    public int TotalCount { get; }

    public int TotalPages { get; }

    public bool HasPreviousPage => PageIndex > 0;

    public bool HasNextPage => PageIndex + 1 < TotalPages;
}
