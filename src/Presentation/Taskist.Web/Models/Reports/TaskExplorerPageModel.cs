using Microsoft.AspNetCore.Mvc.Rendering;

namespace Taskist.Web.Models.Reports;

public class TaskExplorerPageModel
{
    public IList<SelectListItem> AvailableProjects { get; set; } = [];
}
