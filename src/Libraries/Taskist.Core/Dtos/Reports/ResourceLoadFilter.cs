namespace Taskist.Core.Dtos.Reports;

public class ResourceLoadFilter
{
    public int[] ProjectIds { get; set; } = [];

    public int[] UserIds { get; set; } = [];

    public int[] StatusGroupIds { get; set; } = [];

    public int[] SeverityIds { get; set; } = [];

    public string Role { get; set; } = "both";

    public string DueWindow { get; set; } = "all";

    public DateOnly? From { get; set; }

    public DateOnly? To { get; set; }

    public string LoadBasis { get; set; } = "assignee";

    public bool IncludeUnassigned { get; set; }

    public bool Weighted { get; set; }

    public int? BusyThreshold { get; set; }
}
