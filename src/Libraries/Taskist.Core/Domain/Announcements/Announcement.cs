using Taskist.Core.Domain.Common;
using Taskist.Core.Enums;

namespace Taskist.Core.Domain.Announcements;

public class Announcement : BaseEntity, ISoftDeletedEntity, IAuditableEntity
{
    public string Title { get; set; }

    public string Body { get; set; }

    public AnnouncementType Type { get; set; }

    public AnnouncementSeverity Severity { get; set; }

    public AnnouncementAudience Audience { get; set; }

    public DateOnly ValidFrom { get; set; }

    public DateOnly ValidTo { get; set; }

    public bool RequiresAcknowledgement { get; set; }

    public int DisplayOrder { get; set; }

    public bool Active { get; set; }

    public bool Deleted { get; set; }

    public virtual ICollection<AnnouncementRole> Roles { get; set; }
}
