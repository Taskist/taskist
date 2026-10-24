using Taskist.Core.Dtos.Git;

namespace Taskist.Web.Models.WorkItems;

public class TaskCodeViewModel
{
	public TaskCodeDto Code { get; set; } = new();

	public int TaskId { get; set; }

	public int ProjectId { get; set; }

	public bool CanManageBranch { get; set; }

	public bool CanCreateBranch { get; set; }

	public int? ClosingMrId { get; set; }

	public int? ClosingMrRepositoryId { get; set; }

	public string ClosingRefPrefix { get; set; } = "!";

	public bool CanReview { get; set; }

	public IList<int> ReviewRepositoryIds { get; set; } = [];

	public bool IsClosing(TaskCodeDto.MergeRequest mr) =>
		ClosingMrId == mr.Iid && (ClosingMrRepositoryId == null || ClosingMrRepositoryId == mr.RepositoryId);
}
