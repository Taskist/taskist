using Taskist.Core.Domain.Common;
using Taskist.Core.Domain.Masters;
using Taskist.Core.Domain.Users;

namespace Taskist.Core.Domain.WorkItems;

public class Backlog : BaseEntity, ISoftDeletedEntity
{
    public Guid Code { get; set; }

    public string Title { get; set; }

    public string? Description { get; set; }

    public int? ParentId { get; set; }

    public int TaskTypeId { get; set; }

    public int SeverityId { get; set; }

    public DateOnly? DueDate { get; set; }

    public int ProjectId { get; set; }

    public int? ModuleId { get; set; }

    public int? SubModuleId { get; set; }

    public int? SprintId { get; set; }

    public int? ReporterId { get; set; }

    public int? AssigneeId { get; set; }

    public int SubTaskCount { get; set; }

    public int StatusId { get; set; }

    public int CreatedById { get; set; }

    public DateTime CreatedOn { get; set; }

    public int? ModifiedById { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public bool Deleted { get; set; }

    public virtual Backlog Parent { get; set; }

    public virtual TaskType TaskType { get; set; }

    public virtual Severity Severity { get; set; }

    public virtual Project Project { get; set; }

    public virtual Module Module { get; set; }

    public virtual SubModule SubModule { get; set; }

    public virtual Reporter Reporter { get; set; }

    public virtual Sprint Sprint { get; set; }

    public virtual User Assignee { get; set; }

    public virtual Status Status { get; set; }

    public virtual User CreatedBy { get; set; }

    public virtual User ModifiedBy { get; set; }
}
