using Taskist.Core.Domain.Common;
using Taskist.Core.Domain.Users;

namespace Taskist.Core.Domain.Masters;

public class Project : BaseEntity, ISoftDeletedEntity, IAuditableEntity
{
    public string Name { get; set; }

    public string? Description { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public int ClientId { get; set; }

    public int? ProjectManagerId { get; set; }

    public int? TeamLeadId { get; set; }

    public int? CodeReviewerId { get; set; }

    public string? GitProjectReference { get; set; }

    public string? ReleaseVersionFormat { get; set; }

    public int? ReleaseFullIncrement { get; set; }

    public bool Active { get; set; }

    public bool Deleted { get; set; }

    public virtual Client Client { get; set; }

    public virtual User ProjectManager { get; set; }

    public virtual User TeamLead { get; set; }

    public virtual User CodeReviewer { get; set; }
}