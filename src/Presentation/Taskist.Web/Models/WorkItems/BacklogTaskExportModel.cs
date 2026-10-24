using Taskist.Web.Models.Common;

namespace Taskist.Web.Models.WorkItems;

public class BacklogExportModel : BaseModel
{
    public string Title { get; set; }

    public string? Description { get; set; }

    public string? RootCauseAnalysis { get; set; }

    public string? ResolutionDetails { get; set; }

    public string DueDate { get; set; }

    public string Project { get; set; }

    public string? Module { get; set; }

    public string? SubModule { get; set; }

    public string? Release { get; set; }

    public string? Assignee { get; set; }

    public string? Tester { get; set; }

    public int ReOpenCount { get; set; }

    public int SubTaskCount { get; set; }

    public string CreatedBy { get; set; }

    public string CreatedOn { get; set; }

    public int? AgeDays { get; set; }

    public string Status { get; set; }

    public string Severity { get; set; }

    public string TaskType { get; set; }

    #region Tags

    public string? Tags { get; set; }

    #endregion
}
