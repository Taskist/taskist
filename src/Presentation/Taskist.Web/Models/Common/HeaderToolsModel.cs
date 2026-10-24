namespace Taskist.Web.Models.Common;

public class HeaderToolsModel
{
	public bool CanSearch { get; set; }

	public bool CanBacklog { get; set; }

	public bool CanWiki { get; set; }

	public bool CanCreateBacklog => CanBacklog;

	public int OverdueTasks { get; set; }

	public int UnreadNotifications { get; set; }

	public int AlertCount => OverdueTasks + UnreadNotifications;

	public bool HasCreate => CanCreateBacklog || CanWiki;
}
