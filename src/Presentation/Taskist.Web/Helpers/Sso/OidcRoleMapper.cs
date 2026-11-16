namespace Taskist.Web.Helpers.Sso;

public static class OidcRoleMapper
{
    #region Methods

    public static IReadOnlyList<string> MapRoles(IEnumerable<string>? groups, IReadOnlyDictionary<string, string>? mappings)
    {
        if (groups == null || mappings == null || mappings.Count == 0)
            return [];

        var normalized = mappings
            .Where(m => !string.IsNullOrWhiteSpace(m.Key) && !string.IsNullOrWhiteSpace(m.Value))
            .GroupBy(m => m.Key.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Value.Trim(), StringComparer.OrdinalIgnoreCase);

        return groups
            .Where(g => !string.IsNullOrWhiteSpace(g))
            .Select(g => g.Trim())
            .Where(normalized.ContainsKey)
            .Select(g => normalized[g])
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static IReadOnlyList<string> RolesToAdd(IEnumerable<string> mappedRoles, IEnumerable<string> currentRoles)
    {
        var current = new HashSet<string>(currentRoles ?? [], StringComparer.OrdinalIgnoreCase);

        return (mappedRoles ?? [])
            .Where(r => !current.Contains(r))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    #endregion
}
