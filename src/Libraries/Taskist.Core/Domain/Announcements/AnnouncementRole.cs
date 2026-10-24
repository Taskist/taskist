using Taskist.Core.Domain.Common;
using Taskist.Core.Domain.Users;

namespace Taskist.Core.Domain.Announcements;

public class AnnouncementRole : BaseEntity
{
    public int AnnouncementId { get; set; }

    public int UserRoleId { get; set; }

    public virtual Announcement Announcement { get; set; }

    public virtual UserRole UserRole { get; set; }
}
