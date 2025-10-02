using Taskist.Core.Domain.Common;

namespace Taskist.Core.Domain.Masters;

public class Module : BaseEntity, ISoftDeletedEntity
{
    public string Name { get; set; }

    public string? Description { get; set; }

    public int? ProjectId { get; set; }

    public bool Active { get; set; }

    public bool Deleted { get; set; }

    public virtual Project Project { get; set; }
}