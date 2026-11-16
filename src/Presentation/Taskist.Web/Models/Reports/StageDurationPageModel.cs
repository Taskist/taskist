using Microsoft.AspNetCore.Mvc.Rendering;

namespace Taskist.Web.Models.Reports;

public class StageDurationPageModel
{
    public IList<SelectListItem> AvailableProjects { get; set; } = [];

    public IList<SelectListItem> AvailableTaskTypes { get; set; } = [];
}
