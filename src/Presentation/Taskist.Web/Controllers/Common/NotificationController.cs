using Microsoft.AspNetCore.Mvc;
using Taskist.Core.Enums;
using Taskist.Service.Common;
using Taskist.Service.Messages;
using Taskist.Web.Models.Common;

namespace Taskist.Web.Controllers.Common;

public class NotificationController : BaseController
{
	#region Fields

	protected readonly IUserNotificationService _userNotificationService;
	protected readonly IRealtimeAlertNotifier _alertNotifier;

	#endregion

	#region Ctor

	public NotificationController(IUserNotificationService userNotificationService,
		IRealtimeAlertNotifier alertNotifier)
	{
		_userNotificationService = userNotificationService;
		_alertNotifier = alertNotifier;
	}

	#endregion

	#region Actions

	[HttpGet]
	public async Task<IActionResult> Recent()
	{
		var items = await _userNotificationService.GetRecentAsync(UserId);
		return PartialView("_NotificationList", new NotificationListModel
		{
			Items = items,
			Now = DateTime.UtcNow
		});
	}

	[HttpPost]
	public async Task<IActionResult> MarkRead(int id)
	{
		var entity = await _userNotificationService.MarkReadAsync(UserId, id);
		await _alertNotifier.NotifyAsync(UserId);
		return Json(new
		{
			status = entity == null ? HttpStatusCode.NoData : HttpStatusCode.Success,
			url = entity?.Url
		});
	}

	[HttpPost]
	public async Task<IActionResult> MarkAllRead()
	{
		await _userNotificationService.MarkAllReadAsync(UserId);
		await _alertNotifier.NotifyAsync(UserId);
		return Json(new { status = HttpStatusCode.Success });
	}

	#endregion
}
