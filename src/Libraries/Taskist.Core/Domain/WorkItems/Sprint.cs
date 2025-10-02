using Taskist.Core.Domain.Common;
using Taskist.Core.Domain.Masters;

namespace Taskist.Core.Domain.WorkItems;

public class Sprint : BaseEntity, ISoftDeletedEntity
{
	public string Name { get; set; }

	public string? Description { get; set; }

	public int ProjectId { get; set; }

	public DateOnly StartDate { get; set; }

	public DateOnly EndDate { get; set; }

	public bool Started { get; set; }

	public bool Completed { get; set; }

	public bool Deleted { get; set; }

	public virtual Project Project { get; set; }
}
