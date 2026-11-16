using Taskist.Core.Domain.Common;
using Taskist.Core.Domain.Users;

namespace Taskist.Core.Domain.WorkItems;

public class BacklogComment : BaseEntity, IAuditableEntity
{
    public int BacklogId { get; set; }

    public string Comment { get; set; }

    public int CreatedById { get; set; }

    public bool SystemComment { get; set; }

    public bool PostToIssue { get; set; }

    public string? FieldName { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public DateTime CreatedOn { get; set; }

    public int? ModifiedById { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual Backlog Backlog { get; set; }

    public virtual User CreatedBy { get; set; }

    public virtual User ModifiedBy { get; set; }
}
