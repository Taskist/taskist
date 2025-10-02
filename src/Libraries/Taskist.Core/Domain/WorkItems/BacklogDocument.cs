using Taskist.Core.Domain.Common;
using Taskist.Core.Domain.Masters;
using Taskist.Core.Domain.Users;

namespace Taskist.Core.Domain.WorkItems;

public class BacklogDocument : BaseEntity
{
    public int BacklogId { get; set; }

    public int DocumentId { get; set; }

    public int CreatedById { get; set; }

    public DateTime CreatedOn { get; set; }

    public virtual Backlog Backlog { get; set; }

    public virtual Document Document { get; set; }

    public virtual User CreatedBy { get; set; }
}
