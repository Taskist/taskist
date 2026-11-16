using Taskist.Web.Models.Common;
using Taskist.Web.Models.Masters;

namespace Taskist.Web.Models.WorkItems;

public class BacklogItemGridModel : BaseModel
{
	public string Title { get; set; }

	public DateOnly? DueDate { get; set; }

	public int ProjectId { get; set; }

	public string Project { get; set; }

	public int? ModuleId { get; set; }

	public string? Module { get; set; }

	public int? SubModuleId { get; set; }

	public string? SubModule { get; set; }

	public int? ReleaseId { get; set; }

	public string? Release { get; set; }

	public int? AssigneeId { get; set; }

	public string? Assignee { get; set; }

	public int? TesterId { get; set; }

	public string? Tester { get; set; }

	public int ReOpenCount { get; set; }

	public int SubTaskCount { get; set; }

	public int AttachmentCount { get; set; }

	public int CommentCount { get; set; }

	public int? MrId { get; set; }

	public bool HasClosingComment { get; set; }

	public List<string> BranchNames { get; set; } = [];

	public string CreatedBy { get; set; }

	public DateTime CreatedOn { get; set; }

	public DateTime? StatusSince { get; set; }

	public DateTime? OpenSince { get; set; }

	public int AgeDays { get; set; }

	public int StatusDays { get; set; }

	public string? Reporter { get; set; }

	public string? ModifiedBy { get; set; }

	public DateTime? ModifiedOn { get; set; }

	public StatusModel Status { get; set; }

	public SeverityModel Severity { get; set; }

	public TaskTypeModel TaskType { get; set; }
}
