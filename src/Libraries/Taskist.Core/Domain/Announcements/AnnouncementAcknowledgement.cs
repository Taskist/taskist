using Taskist.Core.Domain.Common;
using Taskist.Core.Domain.Users;

namespace Taskist.Core.Domain.Announcements;

public class AnnouncementAcknowledgement : BaseEntity
{
    public int AnnouncementId { get; set; }

    public int UserId { get; set; }

    public DateTime AcknowledgedOnUtc { get; set; }

    public virtual Announcement Announcement { get; set; }

    public virtual User User { get; set; }
}
