namespace Taskist.Web.Models.Announcements;

public class AnnouncementDisplayModel
{
    public List<AnnouncementDisplayItemModel> Items { get; set; } = [];

    public string OneTimeJson { get; set; } = "[]";
}
