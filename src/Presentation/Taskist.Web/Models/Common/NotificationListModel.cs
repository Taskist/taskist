using Taskist.Core.Domain.Users;

namespace Taskist.Web.Models.Common;

public class NotificationListModel
{
	public IList<UserNotification> Items { get; set; } = [];

	public DateTime Now { get; set; }
}
