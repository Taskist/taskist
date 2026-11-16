using Taskist.Core.Domain.Common;

namespace Taskist.Core.Domain.Masters;

public class NotificationChannel : BaseEntity, ISoftDeletedEntity, IAuditableEntity
{
    public string Name { get; set; }

    public int ChannelType { get; set; }

    [NotAudited]
    [Encrypted]
    public string EndpointUrl { get; set; }

    [NotAudited]
    [Encrypted]
    public string? SigningSecret { get; set; }

    public bool Active { get; set; }

    public bool Deleted { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public DateTime? LastSentOn { get; set; }

    public string? LastError { get; set; }
}
