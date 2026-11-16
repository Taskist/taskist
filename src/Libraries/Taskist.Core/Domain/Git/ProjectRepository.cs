using Taskist.Core.Domain.Common;
using Taskist.Core.Domain.Masters;
using Taskist.Core.Domain.Users;

namespace Taskist.Core.Domain.Git;

public class ProjectRepository : BaseEntity, ISoftDeletedEntity, IAuditableEntity
{
    public int ProjectId { get; set; }

    public int GitConnectionId { get; set; }

    public string RepoRef { get; set; }

    public string? ExternalRepoId { get; set; }

    public string? DisplayName { get; set; }

    public string? WebUrl { get; set; }

    public string? DefaultBranch { get; set; }

    public bool? IsPrivate { get; set; }

    public bool IsPrimary { get; set; }

    public string WebhookKey { get; set; }

    [NotAudited]
    [Encrypted]
    public string? WebhookSecret { get; set; }

    public DateTime? WebhookSecretCreatedOn { get; set; }

    public DateTime? WebhookLastDeliveryOn { get; set; }

    public int IssueSyncMode { get; set; }

    public DateTime? IssueSyncEnabledOn { get; set; }

    public string? IssueImportLabel { get; set; }

    public string? IssueSyncTaskTypeIds { get; set; }

    public bool IssueSyncComments { get; set; }

    public int? IssueTaskTypeId { get; set; }

    public int? IssueSeverityId { get; set; }

    public int? IssueOpenStatusId { get; set; }

    public int? IssueClosedStatusId { get; set; }

    public int? IssueSyncUserId { get; set; }

    public int IssueImportLimitPerHour { get; set; }

    public DateTime? IssueLastPolledOn { get; set; }

    public bool Active { get; set; }

    public bool Deleted { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual Project Project { get; set; }

    public virtual GitConnection GitConnection { get; set; }

    public virtual TaskType IssueTaskType { get; set; }

    public virtual Severity IssueSeverity { get; set; }

    public virtual Status IssueOpenStatus { get; set; }

    public virtual Status IssueClosedStatus { get; set; }

    public virtual User IssueSyncUser { get; set; }
}
