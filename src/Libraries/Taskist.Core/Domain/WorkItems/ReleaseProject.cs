using Taskist.Core.Domain.Common;
using Taskist.Core.Domain.Masters;

namespace Taskist.Core.Domain.WorkItems;

public class ReleaseProject : BaseEntity
{
	public int ReleaseId { get; set; }

	public int ProjectId { get; set; }

	public virtual Release Release { get; set; }

	public virtual Project Project { get; set; }
}
