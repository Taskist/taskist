using Microsoft.AspNetCore.Mvc;
using Taskist.Core.Common;
using Taskist.Service.Security;
using Taskist.Service.Masters;
using Taskist.Service.WorkItems;
using Taskist.Web.Helpers.Attributes;

namespace Taskist.Web.Controllers.Common;

/// <summary>
/// Backs the top-bar global search. Returns a flat list of results grouped
/// client-side by the "group" field: matching tasks + navigable menu pages.
/// </summary>
public class SearchController : BaseController
{
    #region Fields

    private const int GroupLimit = 6;

    private readonly IWorkContext _workContext;
    private readonly IMenuService _menuService;
    private readonly IBacklogItemService _backlogService;
    private readonly IPermissionService _permissionService;

    #endregion

    #region Ctor

    public SearchController(IWorkContext workContext,
        IMenuService menuService,
        IBacklogItemService backlogService,
        IPermissionService permissionService)
    {
        _workContext = workContext;
        _menuService = menuService;
        _backlogService = backlogService;
        _permissionService = permissionService;
    }

    #endregion

    #region Actions

    [HttpGet]
    [CheckPermission(PermissionProvider.General.MANAGE_DASHBOARD)]
    public async Task<IActionResult> Query(string q)
    {
        var results = new List<object>();

        q = q?.Trim();
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return Json(results);

        var user = await _workContext.GetCurrentUserAsync();

        // ---- Navigate: match accessible leaf menus by localized name ----
        var menus = await _menuService.GetAllAsync(user);
        var pages = menus
            .Where(m => !string.IsNullOrEmpty(m.ControllerName)
                        && !string.IsNullOrEmpty(m.ActionName)
                        && !string.IsNullOrEmpty(m.Name)
                        && m.Name.Contains(q, StringComparison.OrdinalIgnoreCase))
            .OrderBy(m => m.Name)
            .Take(GroupLimit);

        foreach (var m in pages)
        {
            results.Add(new
            {
                group = "Navigate",
                label = m.Name,
                sublabel = (string)null,
                url = Url.Action(m.ActionName, m.ControllerName),
                icon = string.IsNullOrEmpty(m.Icon) ? "fas fa-diamond-turn-right" : m.Icon
            });
        }

        // ---- Tasks: match accessible backlog items by #id or title ----
        if (await _permissionService.AuthorizeAsync(PermissionProvider.WorkItem.MANAGE_BACKLOGLOG, user))
        {
            var tasks = await _backlogService.GetAccessibleTasksAsync(user.Id);
            var idMatch = int.TryParse(q.TrimStart('#'), out var wantedId);

            var matched = tasks
                .Where(t => (idMatch && t.Id == wantedId)
                            || (!string.IsNullOrEmpty(t.Title) && t.Title.Contains(q, StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(t => idMatch && t.Id == wantedId) // exact id first
                .ThenByDescending(t => t.Id)
                .Take(GroupLimit);

            foreach (var t in matched)
            {
                results.Add(new
                {
                    group = "Tasks",
                    label = $"#{t.Id} {t.Title}",
                    sublabel = t.Project?.Name,
                    url = $"{Url.Action("Edit", "Backlog")}/{t.Id}",
                    icon = "fas fa-list-check"
                });
            }
        }

        return Json(results);
    }

    #endregion
}
