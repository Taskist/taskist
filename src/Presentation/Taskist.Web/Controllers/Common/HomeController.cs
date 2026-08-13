using Microsoft.AspNetCore.Mvc;
using Taskist.Service.Security;
using Taskist.Web.Helpers.Attributes;

namespace Taskist.Web.Controllers.Common;

public class HomeController : BaseController
{
    public async Task<IActionResult> Index()
    {
        return View();
    }

    /// <summary>
    /// Returns just the dashboard widgets for the given filter, for AJAX refresh.
    /// </summary>
    [HttpGet]
    [CheckPermission(PermissionProvider.General.MANAGE_DASHBOARD)]
    public IActionResult DashboardContent(int projectId = 0, int days = 0)
    {
        return ViewComponent("DashboardWidgets", new { projectId, days });
    }
}
