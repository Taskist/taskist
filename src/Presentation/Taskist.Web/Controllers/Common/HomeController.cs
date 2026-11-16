using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Taskist.Core.Common;
using Taskist.Service.Git;
using Taskist.Service.Masters;
using Taskist.Service.Security;
using Taskist.Service.Users;
using Taskist.Web.Models.Dashboard;

namespace Taskist.Web.Controllers.Common;

public class HomeController : BaseController
{
    #region Fields

    protected readonly IPermissionService _permissionService;
    protected readonly IUserService _userService;
    protected readonly IProjectService _projectService;
    protected readonly IWorkContext _workContext;
    protected readonly IGitRepositoryService _gitRepositoryService;
    protected readonly IGitAccountService _gitAccountService;

    #endregion

    #region Ctor

    public HomeController(IPermissionService permissionService,
        IUserService userService,
        IProjectService projectService,
        IWorkContext workContext,
        IGitRepositoryService gitRepositoryService,
        IGitAccountService gitAccountService)
    {
        _gitRepositoryService = gitRepositoryService;
        _gitAccountService = gitAccountService;
        _permissionService = permissionService;
        _userService = userService;
        _projectService = projectService;
        _workContext = workContext;
    }

    #endregion

    #region Actions

    public async Task<IActionResult> Index(int projectId = 0, int days = 90)
    {
        if (!await _permissionService.AuthorizeAsync(PermissionProvider.ManageDashboard))
            return AccessDenied();

        return View(await BuildModelAsync(projectId, days));
    }

    public async Task<IActionResult> Content(int projectId = 0, int days = 90)
    {
        if (!await _permissionService.AuthorizeAsync(PermissionProvider.ManageDashboard))
            return AccessDenied();

        return PartialView("_DashboardContent", await BuildModelAsync(projectId, days));
    }

    public async Task<IActionResult> OrgContent(int days = 90)
    {
        if (!await _permissionService.AuthorizeAsync(PermissionProvider.ViewExecutiveDashboard))
            return AccessDenied();

        var model = await BuildModelAsync(0, days);
        model.ProjectIds = (await _projectService.GetAllActiveAsync()).Select(p => p.Id).ToArray();

        return PartialView("_ExecutiveDashboard", model);
    }

    #endregion

    #region Helper

    private async Task<DashboardModel> BuildModelAsync(int projectId, int days)
    {
        if (days != 30 && days != 90 && days != 180)
            days = 90;

        var projects = await _userService.GetAllAccessibleProjects(UserId);
        if (projectId > 0 && projects.All(p => p.Id != projectId))
            projectId = 0;

        var accessibleIds = projects.Select(p => p.Id).ToArray();
        var canBacklog = await _permissionService.AuthorizeAsync(PermissionProvider.ManageBacklog);
        var user = canBacklog && await _gitRepositoryService.AnyConfiguredAsync() ? await _workContext.GetCurrentUserAsync() : null;

        return new DashboardModel
        {
            IsExecutive = await _permissionService.AuthorizeAsync(PermissionProvider.ViewExecutiveDashboard),
            SelectedProjectId = projectId,
            Days = days,
            ProjectIds = projectId > 0 ? [projectId] : accessibleIds,
            AvailableProjects = projects.Select(p => new SelectListItem
            {
                Text = p.Name,
                Value = p.Id.ToString(),
                Selected = p.Id == projectId
            }).ToList(),
            CanBacklog = canBacklog,
            ShowMyCode = user != null && (!string.IsNullOrWhiteSpace(user.GitUserName) || (await _gitAccountService.GetConnectedConnectionIdsAsync(user.Id)).Count > 0),
            CanRelease = await _permissionService.AuthorizeAsync(PermissionProvider.ManageRelease),
            CanAnalytics = await _permissionService.AuthorizeAsync(PermissionProvider.ManageAnalyticsReport)
        };
    }

    #endregion
}
