using Taskist.Core.Domain.Common;
using Taskist.Core.Domain.WorkItems;

namespace Taskist.Core.Domain.Git;

public class ExternalIssueLink : BaseEntity
{
    public int BacklogId { get; set; }

    public int ProjectRepositoryId { get; set; }

    public string ExternalId { get; set; }

    public int Number { get; set; }

    public string? Url { get; set; }

    public string? RemoteState { get; set; }

    public string? Snapshot { get; set; }

    public bool SyncEnabled { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? LastSyncedOn { get; set; }

    public DateTime? RemoteUpdatedOn { get; set; }

    public string? LastError { get; set; }

    public virtual Backlog Backlog { get; set; }

    public virtual ProjectRepository ProjectRepository { get; set; }
}
