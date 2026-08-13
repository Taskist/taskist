using System.Reflection;
using System.Linq.Dynamic.Core;
using System.Collections.Concurrent;

namespace Taskist.Data.Extensions;

/// <summary>
/// Safe ordering helpers for grid queries.
/// <para>
/// Sort column and direction arrive from the client as free text and are handed to
/// Dynamic LINQ, which interprets them as an expression. Only names that match a real
/// property of the entity are accepted so the caller cannot inject an expression.
/// </para>
/// </summary>
public static class SortingExtensions
{
    #region Field

    private static readonly ConcurrentDictionary<Type, HashSet<string>> _propertyCache = new();

    private const string Ascending = "asc";

    private const string Descending = "desc";

    #endregion

    #region Utilities

    /// <summary>
    /// Returns the public instance property names declared by the entity.
    /// </summary>
    private static HashSet<string> GetSortableProperties(Type type)
    {
        return _propertyCache.GetOrAdd(type, key => key
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.CanRead && property.GetIndexParameters().Length == 0)
            .Select(property => property.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Resolves the supplied name to its declared casing, or null when unknown.
    /// </summary>
    private static string ResolveColumn<T>(string sortColumn)
    {
        if (string.IsNullOrWhiteSpace(sortColumn))
            return null;

        //nested paths such as "Project.Name" are validated one segment at a time
        var currentType = typeof(T);
        var resolved = new List<string>();

        foreach (var segment in sortColumn.Split('.', StringSplitOptions.RemoveEmptyEntries))
        {
            var properties = currentType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var match = properties.FirstOrDefault(property =>
                property.Name.Equals(segment.Trim(), StringComparison.OrdinalIgnoreCase) &&
                property.CanRead &&
                property.GetIndexParameters().Length == 0);

            if (match == null)
                return null;

            resolved.Add(match.Name);
            currentType = match.PropertyType;
        }

        return resolved.Count == 0 ? null : string.Join('.', resolved);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Orders the query by a client supplied column, ignoring anything that is not
    /// a real property. Falls back to <paramref name="defaultSortColumn"/> (or Id).
    /// </summary>
    public static IQueryable<T> OrderBySafe<T>(this IQueryable<T> query,
        string sortColumn,
        string sortDirection,
        string defaultSortColumn = "Id")
    {
        var column = ResolveColumn<T>(sortColumn) ?? ResolveColumn<T>(defaultSortColumn);

        //nothing sortable on this entity - leave the query untouched
        if (column == null)
            return query;

        var direction = string.Equals(sortDirection?.Trim(), Descending, StringComparison.OrdinalIgnoreCase)
            ? Descending
            : Ascending;

        return query.OrderBy($"{column} {direction}");
    }

    /// <summary>
    /// Orders the query by several client supplied columns, dropping unknown names.
    /// </summary>
    public static IQueryable<T> OrderBySafe<T>(this IQueryable<T> query, IEnumerable<string> sortColumns)
    {
        var columns = (sortColumns ?? [])
            .Select(ResolveColumn<T>)
            .Where(column => column != null)
            .ToList();

        return columns.Count == 0 ? query : query.OrderBy(string.Join(",", columns));
    }

    #endregion
}
