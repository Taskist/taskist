using Microsoft.AspNetCore.Mvc.Rendering;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Models.Common;

namespace Taskist.Web.Models.Masters;

public class ReporterModel : BaseModel
{
    public ReporterModel()
    {
        AvailableProjects = [];
    }

    [LocalizedDisplayName("ReporterModel.Name")]
    public string Name { get; set; }

    [LocalizedDisplayName("ReporterModel.Description")]
    public string? Description { get; set; }

    [LocalizedDisplayName("ReporterModel.Project")]
    public int? ProjectId { get; set; }

    [LocalizedDisplayName("ReporterModel.Active")]
    public bool Active { get; set; } = true;

    public IList<SelectListItem> AvailableProjects { get; set; }
}
