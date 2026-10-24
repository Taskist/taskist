using Taskist.Core.Domain.Common;
using Taskist.Core.Domain.Git;
using Taskist.Core.Domain.Masters;
using Taskist.Core.Domain.Users;

namespace Taskist.Core.Domain.WorkItems;

public class GitBranch : BaseEntity, ISoftDeletedEntity
{
	public int ProjectId { get; set; }

	public int? ProjectRepositoryId { get; set; }

	public string GitProjectReference { get; set; }

	public string Name { get; set; }

	public string? BaseBranch { get; set; }

	public int? Origin { get; set; }

	public int CreatedById { get; set; }

	public DateTime CreatedOn { get; set; }

	public bool Deleted { get; set; }

	public virtual Project Project { get; set; }

	public virtual ProjectRepository ProjectRepository { get; set; }

	public virtual User CreatedBy { get; set; }

	public virtual ICollection<GitBranchTask> Tasks { get; set; } = new List<GitBranchTask>();
}
