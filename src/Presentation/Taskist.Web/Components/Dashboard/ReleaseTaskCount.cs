using Taskist.Core.Common;
using Taskist.Service.WorkItems;
using Microsoft.AspNetCore.Mvc;

namespace Taskist.Web.Components.Dashboard;

public class ReleaseTaskCount : ViewComponent
{
    #region Fields

    protected readonly IBacklogService _backlogService;
    protected readonly IWorkContext _workContext;

    #endregion

    #region Ctor

    public ReleaseTaskCount(IBacklogService backlogService,
        IWorkContext workContext)
    {
        _backlogService = backlogService;
        _workContext = workContext;
    }

    #endregion

    #region Methods

    public async Task<IViewComponentResult> InvokeAsync(int[] projectIds)
    {
        var userId = _workContext.GetCurrentUserId() ?? 0;
        var model = await _backlogService.GetReleaseTaskDataAsync(userId, projectIds);
        return View(model);
    }

    #endregion
}