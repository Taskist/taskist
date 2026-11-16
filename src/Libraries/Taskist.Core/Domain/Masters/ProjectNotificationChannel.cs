using Taskist.Core.Domain.Common;

namespace Taskist.Core.Domain.Masters;

public class ProjectNotificationChannel : BaseEntity, IAuditableEntity
{
    public int ProjectId { get; set; }

    public int NotificationChannelId { get; set; }

    public int Events { get; set; }

    public bool Active { get; set; }

    public virtual Project Project { get; set; }

    public virtual NotificationChannel NotificationChannel { get; set; }
}
