using Taskist.Core.Domain.Common;
using Taskist.Core.Domain.WorkItems;

namespace Taskist.Core.Domain.Git;

public class ExternalCommentLink : BaseEntity
{
    public int ExternalIssueLinkId { get; set; }

    public int BacklogCommentId { get; set; }

    public string ExternalCommentId { get; set; }

    public bool Inbound { get; set; }

    public string? ContentHash { get; set; }

    public DateTime LastSyncedOn { get; set; }

    public virtual ExternalIssueLink ExternalIssueLink { get; set; }

    public virtual BacklogComment BacklogComment { get; set; }
}
