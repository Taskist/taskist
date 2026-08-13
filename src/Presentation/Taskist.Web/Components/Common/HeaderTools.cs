using Microsoft.AspNetCore.Mvc;
using Taskist.Service.Security;
using Taskist.Web.Models.Common;

namespace Taskist.Web.Components.Common;

/// <summary>
/// Renders the top-bar tools: global search + the permission-gated quick-add ("+") menu.
/// </summary>
public class HeaderTools : ViewComponent
{
    #region Fields

    private readonly IPermissionService _permissionService;

    #endregion

    #region Ctor

    public HeaderTools(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    #endregion

    #region Methods

    /// <param name="section">"search" renders the global search, "create" renders the quick-add menu.
    /// Defaults to both (kept for backwards compatibility).</param>
    public async Task<IViewComponentResult> InvokeAsync(string section = "all")
    {
        var model = new HeaderToolsModel
        {
            Section = section,
            CanSearch = await _permissionService.AuthorizeAsync(PermissionProvider.General.MANAGE_DASHBOARD),
            CanCreateTask = await _permissionService.AuthorizeAsync(PermissionProvider.WorkItem.MANAGE_BACKLOGLOG),
            CanCreateProject = await _permissionService.AuthorizeAsync(PermissionProvider.Configuration.MANAGE_PROJECT),
            CanCreateClient = await _permissionService.AuthorizeAsync(PermissionProvider.Configuration.MANAGE_CLIENT)
        };

        return View(model);
    }

    #endregion
}
