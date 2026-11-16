namespace Taskist.Core.Dtos.WorkItems;

public class ProjectOpenStatusSummaryDto
{
    public int ProjectId { get; set; }

    public string ProjectName { get; set; }

    public int? ProjectManagerId { get; set; }

    public int? TeamLeadId { get; set; }

    public int TotalOpen { get; set; }

    public List<StatusCount> Statuses { get; set; } = [];
}
