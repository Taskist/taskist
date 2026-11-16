using Taskist.Service.Reports;
using Microsoft.AspNetCore.Mvc;

namespace Taskist.Web.Components.Dashboard;

public class WorkTrend : ViewComponent
{
    #region Fields

    protected readonly IReportService _reportService;

    #endregion

    #region Ctor

    public WorkTrend(IReportService reportService)
    {
        _reportService = reportService;
    }

    #endregion

    #region Methods

    public async Task<IViewComponentResult> InvokeAsync(int[] projectIds, int days)
    {
        var months = Math.Max(1, (int)Math.Ceiling(days / 30.0));
        var model = await _reportService.GetCreatedVsResolvedAsync(projectIds ?? [], months);
        return View(model);
    }

    #endregion
}
