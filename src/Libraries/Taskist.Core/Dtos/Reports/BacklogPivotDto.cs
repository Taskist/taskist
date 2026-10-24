namespace Taskist.Core.Dtos.Reports;

public class BacklogPivotDto
{
    public string TaskType { get; set; }

    public string Severity { get; set; }

    public DateOnly? DueDate { get; set; }

    public string Project { get; set; }

    public string? Module { get; set; }

    public string? SubModule { get; set; }

    public string? Release { get; set; }

    public string? Reporter { get; set; }

    public string? Assignee { get; set; }

    public string? Tester { get; set; }

    public int ReOpenCount { get; set; }

    public string Status { get; set; }

    public string StatusGroup { get; set; }

    public string CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }
}
