using Microsoft.AspNetCore.Mvc;
using Taskist.Core.Common;
using Taskist.Core.Domain.Masters;
using Taskist.Service.Localization;
using Taskist.Service.Masters;
using Taskist.Service.Security;
using Taskist.Service.Users;
using Taskist.Service.WorkItems;

namespace Taskist.Web.Controllers.Common;

public class SearchController : BaseController
{
    #region Fields

    protected readonly IBacklogService _backlogService;
    protected readonly IUserService _userService;
    protected readonly IPermissionService _permissionService;
    protected readonly IMenuService _menuService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IWorkContext _workContext;

    #endregion

    #region Ctor

    public SearchController(IBacklogService backlogService,
        IUserService userService,
        IPermissionService permissionService,
        IMenuService menuService,
        ILocalizationService localizationService,
        IWorkContext workContext)
    {
        _backlogService = backlogService;
        _userService = userService;
        _permissionService = permissionService;
        _menuService = menuService;
        _localizationService = localizationService;
        _workContext = workContext;
    }

    #endregion

    #region Actions

    [HttpGet]
    public async Task<IActionResult> Query(string q)
    {
        var results = new List<object>();

        if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
            return Json(results);

        q = q.Trim();

        var user = await _workContext.GetCurrentUserAsync();
        if (user != null)
        {
            var menus = await _menuService.GetAllAsync(user);

            var resources = await _localizationService.GetAllMenuResourceAsync(user.LanguageId);
            var labelByKey = new Dictionary<string, string>();
            foreach (var r in resources)
                labelByKey[r.ResourceKey.ToLowerInvariant()] = r.ResourceValue;

            string Localized(Menu m)
            {
                var key = $"{m.SystemName}.menu".ToLowerInvariant();
                return labelByKey.TryGetValue(key, out var v) && !string.IsNullOrWhiteSpace(v) ? v : m.Name;
            }

            var localizedByCode = menus.ToDictionary(m => m.Code, Localized);

            var pages = menus
                .Where(m => !string.IsNullOrEmpty(m.ControllerName) && !string.IsNullOrEmpty(m.ActionName))
                .Where(m => (m.Name ?? string.Empty).Contains(q, StringComparison.OrdinalIgnoreCase)
                    || Localized(m).Contains(q, StringComparison.OrdinalIgnoreCase))
                .Take(6);

            foreach (var m in pages)
                results.Add(new
                {
                    group = "Navigate",
                    label = Localized(m),
                    sublabel = m.ParentCode.HasValue && localizedByCode.TryGetValue(m.ParentCode.Value, out var parent) ? parent : string.Empty,
                    url = Url.Action(m.ActionName, m.ControllerName),
                    icon = string.IsNullOrEmpty(m.Icon) ? "fas fa-diamond-turn-right" : m.Icon
                });
        }

        if (await _permissionService.AuthorizeAsync(PermissionProvider.ManageBacklog))
        {
            var accessibleIds = (await _userService.GetAllAccessibleProjects(UserId)).Select(p => p.Id).ToHashSet();
            var tasks = (await _backlogService.SearchByTitleAsync(q))
                .Where(t => accessibleIds.Contains(t.ProjectId))
                .Take(6);

            foreach (var t in tasks)
                results.Add(new
                {
                    group = "Tasks",
                    label = $"#{t.Id} {t.Title}",
                    sublabel = t.Project?.Name ?? string.Empty,
                    url = Url.Action("Edit", "Backlog", new { id = t.Id }),
                    icon = "fas fa-list-check"
                });
        }

        if (await _permissionService.AuthorizeAsync(PermissionProvider.ManageUser))
        {
            var people = await _userService.GetPagedListAsync(q, 0, 6);

            foreach (var e in people)
                results.Add(new
                {
                    group = "People",
                    label = e.Name,
                    sublabel = e.Email ?? string.Empty,
                    url = Url.Action("Edit", "User", new { id = e.Id }),
                    icon = "fas fa-user"
                });
        }

        return Json(results);
    }

    #endregion
}
