using Taskist.Service.Reports;
using Microsoft.AspNetCore.Mvc;

namespace Taskist.Web.Components.Dashboard;

public class WorkDistribution : ViewComponent
{
    #region Fields

    protected readonly IReportService _reportService;

    #endregion

    #region Ctor

    public WorkDistribution(IReportService reportService)
    {
        _reportService = reportService;
    }

    #endregion

    #region Methods

    public async Task<IViewComponentResult> InvokeAsync(int[] projectIds)
    {
        var model = await _reportService.GetAssigneeWorkloadAsync(projectIds ?? []);
        return View(model);
    }

    #endregion
}
