using Taskist.Core.Domain.Common;
using Taskist.Core.Domain.Masters;
using Taskist.Core.Domain.Users;

namespace Taskist.Core.Domain.WorkItems;

public class BacklogStatusLog : BaseEntity
{
    public int BacklogId { get; set; }

    public int StatusId { get; set; }

    public int CreatedById { get; set; }

    public DateTime CreatedOn { get; set; }

    public virtual Backlog Backlog { get; set; }

    public virtual User CreatedBy { get; set; }

    public virtual Status Status { get; set; }
}
