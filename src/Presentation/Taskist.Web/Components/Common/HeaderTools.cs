using Taskist.Core.Common;
using Taskist.Service.Messages;
using Taskist.Service.Security;
using Taskist.Service.WorkItems;
using Taskist.Web.Models.Common;
using Microsoft.AspNetCore.Mvc;

namespace Taskist.Web.Components.Common;

public class HeaderTools : ViewComponent
{
	#region Fields

	protected readonly IPermissionService _permissionService;
	protected readonly IWorkContext _workContext;
	protected readonly IBacklogService _backlogService;
	protected readonly IUserNotificationService _userNotificationService;

	#endregion

	#region Ctor

	public HeaderTools(IPermissionService permissionService,
		IWorkContext workContext,
		IBacklogService backlogService,
		IUserNotificationService userNotificationService)
	{
		_userNotificationService = userNotificationService;
		_permissionService = permissionService;
		_workContext = workContext;
		_backlogService = backlogService;
	}

	#endregion

	#region Methods

	public async Task<IViewComponentResult> InvokeAsync()
	{
		var userId = _workContext.GetCurrentUserId() ?? 0;

		var model = new HeaderToolsModel
		{
			CanBacklog = await _permissionService.AuthorizeAsync(PermissionProvider.ManageBacklog),
			CanWiki = await _permissionService.AuthorizeAsync(PermissionProvider.ManageWiki)
		};

		var canUsers = await _permissionService.AuthorizeAsync(PermissionProvider.ManageUser);
		model.CanSearch = model.CanBacklog || canUsers;

		if (model.CanBacklog && userId > 0)
		{
			var due = await _backlogService.GetDueTasksAsync(userId);
			model.OverdueTasks = due.GetValueOrDefault("Overdue", 0);
		}

		if (userId > 0)
			model.UnreadNotifications = await _userNotificationService.GetUnreadCountAsync(userId);

		return View(model);
	}

	#endregion
}
