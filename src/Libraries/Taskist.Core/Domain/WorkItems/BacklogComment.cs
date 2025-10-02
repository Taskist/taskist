using Taskist.Core.Domain.Common;
using Taskist.Core.Domain.Users;

namespace Taskist.Core.Domain.WorkItems;

public class BacklogComment : BaseEntity
{
    public int BacklogId { get; set; }

    public string Comment { get; set; }

    public int CreatedById { get; set; }

    public bool SystemComment { get; set; }

    public DateTime CreatedOn { get; set; }

    public int? ModifiedById { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual Backlog Backlog { get; set; }

    public virtual User CreatedBy { get; set; }

    public virtual User ModifiedBy { get; set; }
}
