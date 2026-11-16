using Taskist.Core.Enums;

namespace Taskist.Web.Models.Announcements;

public class AnnouncementDisplayItemModel
{
    public int Id { get; set; }

    public string Title { get; set; }

    public string Body { get; set; }

    public AnnouncementType Type { get; set; }

    public AnnouncementSeverity Severity { get; set; }

    public bool RequiresAcknowledgement { get; set; }
}
