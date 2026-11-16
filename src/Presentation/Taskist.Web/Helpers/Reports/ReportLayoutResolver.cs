using Newtonsoft.Json;
using Taskist.Core.Enums;

namespace Taskist.Web.Helpers.Reports;

public static class ReportLayoutResolver
{
    public static Dictionary<string, List<ReportColumn>> Parse(string layoutsJson)
    {
        if (string.IsNullOrWhiteSpace(layoutsJson))
            return [];

        try
        {
            return JsonConvert.DeserializeObject<Dictionary<string, List<ReportColumn>>>(layoutsJson) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public static List<ReportColumn> GetEditableLayout(string layoutsJson, ReportExportKey key)
    {
        var registry = ReportColumnRegistry.GetColumns(key);
        var saved = Parse(layoutsJson);

        if (!saved.TryGetValue(key.ToString(), out var savedColumns) || savedColumns.Count == 0)
            return registry.Select(c => new ReportColumn { Key = c.Key, Title = c.Title, Included = true }).ToList();

        var registryKeys = registry.Select(c => c.Key).ToHashSet();
        var result = new List<ReportColumn>();

        foreach (var col in savedColumns.Where(c => registryKeys.Contains(c.Key)))
        {
            var defaults = registry.First(c => c.Key == col.Key);
            result.Add(new ReportColumn
            {
                Key = col.Key,
                Title = string.IsNullOrWhiteSpace(col.Title) ? defaults.Title : col.Title,
                Included = col.Included
            });
        }

        var present = result.Select(c => c.Key).ToHashSet();
        foreach (var col in registry.Where(c => !present.Contains(c.Key)))
            result.Add(new ReportColumn { Key = col.Key, Title = col.Title, Included = true });

        return result;
    }

    public static List<ReportColumn> GetExportLayout(string layoutsJson, ReportExportKey key)
    {
        return GetEditableLayout(layoutsJson, key).Where(c => c.Included).ToList();
    }

    public static List<ReportColumn> ParseUserLayout(string userJson)
    {
        if (string.IsNullOrWhiteSpace(userJson))
            return [];

        try
        {
            return JsonConvert.DeserializeObject<List<ReportColumn>>(userJson) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public static List<ReportColumn> ApplyUserLayout(IReadOnlyList<ReportColumn> defaults, IEnumerable<ReportColumn> userColumns)
    {
        var byKey = defaults.ToDictionary(c => c.Key);
        var result = new List<ReportColumn>();
        var present = new HashSet<string>();

        foreach (var col in userColumns ?? [])
        {
            if (col?.Key == null || !byKey.TryGetValue(col.Key, out var source) || !present.Add(col.Key))
                continue;

            result.Add(new ReportColumn { Key = source.Key, Title = source.Title, Included = col.Included });
        }

        foreach (var col in defaults.Where(c => !present.Contains(c.Key)))
            result.Add(new ReportColumn { Key = col.Key, Title = col.Title, Included = col.Included });

        return result;
    }

    public static bool SameLayout(IReadOnlyList<ReportColumn> a, IReadOnlyList<ReportColumn> b)
    {
        return a.Count == b.Count && a.Zip(b).All(p => p.First.Key == p.Second.Key && p.First.Included == p.Second.Included);
    }
}
