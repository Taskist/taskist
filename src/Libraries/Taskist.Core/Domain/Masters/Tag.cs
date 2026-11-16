using Taskist.Core.Domain.Common;

namespace Taskist.Core.Domain.Masters;

public class Tag : BaseEntity, ISoftDeletedEntity, IAuditableEntity
{
	public string Name { get; set; }

	public bool Active { get; set; }

	public bool Deleted { get; set; }
}
