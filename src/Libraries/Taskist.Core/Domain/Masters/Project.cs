using Taskist.Core.Domain.Common;

namespace Taskist.Core.Domain.Masters;

public class Project : BaseEntity, ISoftDeletedEntity
{
    public string Name { get; set; }

    public string? Description { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public int ClientId { get; set; }

    public bool Active { get; set; }

    public bool Deleted { get; set; }

    public virtual Client Client { get; set; }
}