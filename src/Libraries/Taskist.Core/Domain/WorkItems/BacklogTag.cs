using Taskist.Core.Domain.Common;
using Taskist.Core.Domain.Masters;

namespace Taskist.Core.Domain.WorkItems;

public class BacklogTag : BaseEntity
{
    public int BacklogId { get; set; }

    public int TagId { get; set; }

    public virtual Backlog Backlog { get; set; }

    public virtual Tag Tag { get; set; }
}
