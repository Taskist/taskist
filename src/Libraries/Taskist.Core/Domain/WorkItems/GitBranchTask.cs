using Taskist.Core.Domain.Common;
using Taskist.Core.Domain.Users;

namespace Taskist.Core.Domain.WorkItems;

public class GitBranchTask : BaseEntity
{
	public int GitBranchId { get; set; }

	public int BacklogId { get; set; }

	public int LinkedById { get; set; }

	public DateTime LinkedOn { get; set; }

	public virtual GitBranch GitBranch { get; set; }

	public virtual Backlog Backlog { get; set; }

	public virtual User LinkedBy { get; set; }
}
