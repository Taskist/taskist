namespace Taskist.Web.Models.Announcements;

public class AnnouncementAckAuditModel
{
    public string Title { get; set; }

    public List<AnnouncementAckRowModel> Rows { get; set; } = [];
}
