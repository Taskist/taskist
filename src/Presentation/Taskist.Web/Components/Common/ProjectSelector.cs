using Microsoft.AspNetCore.Mvc;
using Taskist.Core.Common;
using Taskist.Service.Masters;
using Taskist.Service.Users;
using Taskist.Web.Models.Masters;

namespace Taskist.Web.Components.Common;

public class ProjectSelector : ViewComponent
{
    #region Fields

    protected readonly IWorkContext _workContext;
    protected readonly IUserService _userService;
    protected readonly IGenericAttributeService _genericAttributeService;

    #endregion

    #region Ctor

    public ProjectSelector(IWorkContext workContext,
        IUserService userService,
        IGenericAttributeService genericAttributeService)
    {
        _workContext = workContext;
        _userService = userService;
        _genericAttributeService = genericAttributeService;
    }

    #endregion

    #region Methods

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var user = await _workContext.GetCurrentUserAsync();
        var menusEntities = await _userService.GetAllAccessibleProjects(user.Id);
        var lastAccessedProjectId = await _genericAttributeService.GetAttributeAsync<int>(user, Constant.ActiveProjectSession);

        var model = menusEntities.Select(x => new ProjectSelectorModel
        {
            Id = x.Id,
            Name = x.Name,
            Selected = x.Id == lastAccessedProjectId
        }).ToList();

        return View(model);
    }

    #endregion
}