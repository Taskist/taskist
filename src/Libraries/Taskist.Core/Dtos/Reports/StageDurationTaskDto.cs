namespace Taskist.Core.Dtos.Reports;

public class StageDurationTaskDto
{
    public int TaskId { get; set; }

    public string Title { get; set; }

    public string Project { get; set; }

    public string TaskType { get; set; }

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    public double Days { get; set; }

    public string DurationText { get; set; }
}
