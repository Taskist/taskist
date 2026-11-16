using Microsoft.AspNetCore.Mvc.Rendering;

namespace Taskist.Web.Models.Reports;

public class ResourceLoadPageModel
{
    public ResourceLoadPageModel()
    {
        AvailableProjects = [];
        AvailableUsers = [];
        AvailableSeverities = [];
        AvailableStatusGroups = [];
    }

    public IList<SelectListItem> AvailableProjects { get; set; }

    public IList<SelectListItem> AvailableUsers { get; set; }

    public IList<SelectListItem> AvailableSeverities { get; set; }

    public IList<SelectListItem> AvailableStatusGroups { get; set; }
}
