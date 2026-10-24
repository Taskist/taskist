using Taskist.Core.Dtos.Git;

namespace Taskist.Web.Models.WorkItems;

public class TaskIssueViewModel
{
	public int TaskId { get; set; }

	public TaskIssueDto Issue { get; set; } = new();
}
