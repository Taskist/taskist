using Microsoft.AspNetCore.Mvc.Rendering;

namespace Taskist.Web.Models.Dashboard;

public class DashboardModel
{
    public int SelectedProjectId { get; set; }

    public int Days { get; set; } = 90;

    public int[] ProjectIds { get; set; } = [];

    public IList<SelectListItem> AvailableProjects { get; set; } = [];

    #region Permission flags

    public bool CanBacklog { get; set; }

    public bool ShowMyCode { get; set; }

    public bool CanRelease { get; set; }

    public bool CanAnalytics { get; set; }

    public bool IsExecutive { get; set; }

    #endregion
}
