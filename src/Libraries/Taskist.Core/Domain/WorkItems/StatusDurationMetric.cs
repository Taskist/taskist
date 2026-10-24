using Taskist.Core.Domain.Common;
using Taskist.Core.Domain.Masters;

namespace Taskist.Core.Domain.WorkItems;

public class StatusDurationMetric : BaseEntity, ISoftDeletedEntity, IAuditableEntity
{
	public string Name { get; set; }

	public int FromStatusId { get; set; }

	public int ToStatusId { get; set; }

	public int? ProjectId { get; set; }

	public bool ShowOnTask { get; set; }

	public int DisplayOrder { get; set; }

	public bool Active { get; set; }

	public bool Deleted { get; set; }

	public virtual Status FromStatus { get; set; }

	public virtual Status ToStatus { get; set; }

	public virtual Project Project { get; set; }
}
