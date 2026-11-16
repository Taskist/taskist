using Taskist.Core.Domain.Common;
using Taskist.Core.Domain.Masters;

namespace Taskist.Core.Domain.Users;

public class UserProjectMap : BaseEntity, IAuditableEntity
{
    public int UserId { get; set; }

    public int ProjectId { get; set; }

    public bool CanReportToDo { get; set; }

    public bool CanReportCR { get; set; }

    public bool CanReportBug { get; set; }

    public bool CanEdit { get; set; }

    public bool CanClose { get; set; }

    public bool CanReOpen { get; set; }

    public bool CanComment { get; set; }

    public bool CanViewOthersTask { get; set; }

    public bool CanEditOthersTask { get; set; }

    public bool CanAddWiki { get; set; }

    public bool CanDeleteTask { get; set; }

    public bool CanChangeProject { get; set; }

    public bool CanReviewMergeRequests { get; set; }

    public bool CanMergeMergeRequests { get; set; }

    public virtual User User { get; set; }

    public virtual Project Project { get; set; }
}