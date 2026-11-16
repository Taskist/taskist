namespace Taskist.Web.Helpers.Wiki;

public static class WikiTreeRules
{
    #region Methods

    public static bool WouldCreateCycle(int movingCategoryId, int? newParentId, IReadOnlyDictionary<int, int?> parentOf)
    {
        var visited = new HashSet<int>();
        var current = newParentId;
        while (current.HasValue)
        {
            if (current.Value == movingCategoryId || !visited.Add(current.Value))
                return true;

            current = parentOf.TryGetValue(current.Value, out var parent) ? parent : null;
        }

        return false;
    }

    public static bool IsSameProject(int? sourceProjectId, int? targetProjectId) =>
        sourceProjectId.HasValue && targetProjectId.HasValue && sourceProjectId.Value > 0 && sourceProjectId.Value == targetProjectId.Value;

    #endregion
}
