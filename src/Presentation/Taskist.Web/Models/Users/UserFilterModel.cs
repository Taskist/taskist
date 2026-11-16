namespace Taskist.Web.Models.Users;

public class UserFilterModel
{
    #region Raw (CSV) bound from the grid filter controls

    public string? Statuses { get; set; }

    public string? RoleIds { get; set; }

    public string? Locked { get; set; }

    #endregion

    #region Parsed

    public int[] StatusValues => Parse(Statuses);

    public int[] RoleValues => Parse(RoleIds);

    public bool? LockedValue => bool.TryParse(Locked, out var b) ? b : null;

    private static int[] Parse(string? csv)
    {
        if (string.IsNullOrWhiteSpace(csv))
            return [];

        return csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(s => int.TryParse(s, out _))
            .Select(int.Parse)
            .ToArray();
    }

    #endregion
}
