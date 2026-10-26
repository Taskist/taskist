using System.Collections.Concurrent;
using System.Linq.Dynamic.Core;
using System.Reflection;

namespace Taskist.Core.Extensions;

public static class QueryableSortExtensions
{

    private static readonly ConcurrentDictionary<Type, IReadOnlyDictionary<string, string>> _sortableCache = new();

    public static IQueryable<T> ApplySafeSort<T>(this IQueryable<T> query, string? sortColumn,
            string? sortDirection, string defaultColumn = "Id")
    {
        var sortable = GetSortableProperties(typeof(T));
        if (sortable.Count == 0)
            return query;

        if (string.IsNullOrWhiteSpace(sortColumn) || !sortable.TryGetValue(sortColumn.Trim().ToLowerInvariant(), out var column))
        {
            if (!sortable.TryGetValue(defaultColumn.ToLowerInvariant(), out column))
                column = sortable.Values.First();
        }

        var direction = string.Equals(sortDirection?.Trim(), "desc", StringComparison.OrdinalIgnoreCase)
                    ? "desc"
                    : "asc";

        return query.OrderBy($"{column} {direction}");
    }

    private static IReadOnlyDictionary<string, string> GetSortableProperties(Type type) =>
            _sortableCache.GetOrAdd(type, t =>
            {
                var map = new Dictionary<string, string>();

                var props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanRead)
                    .ToList();

                foreach (var p in props.Where(p => IsSortable(p.PropertyType)))
                    map.TryAdd(p.Name.ToLowerInvariant(), p.Name);

                foreach (var nav in props.Where(p => IsNavigation(p.PropertyType)))
                {
                    foreach (var child in nav.PropertyType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                 .Where(c => c.CanRead && IsSortable(c.PropertyType)))
                    {
                        var key = $"{nav.Name}.{child.Name}".ToLowerInvariant();
                        map.TryAdd(key, $"{nav.Name}.{child.Name}");
                    }
                }

                return map;
            });

    private static bool IsNavigation(Type type)
    {
        return type.IsClass
            && type != typeof(string)
            && !typeof(System.Collections.IEnumerable).IsAssignableFrom(type);
    }

    private static bool IsSortable(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;

        return type.IsPrimitive
                    || type.IsEnum
                    || type == typeof(string)
                    || type == typeof(decimal)
                    || type == typeof(DateTime)
                    || type == typeof(DateTimeOffset)
                    || type == typeof(TimeSpan)
                    || type == typeof(Guid);
    }
}
