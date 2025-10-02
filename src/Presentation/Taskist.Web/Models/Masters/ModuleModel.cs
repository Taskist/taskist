using Microsoft.AspNetCore.Mvc.Rendering;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Models.Common;

namespace Taskist.Web.Models.Masters;

public class ModuleModel : BaseModel
{
    public ModuleModel()
    {
        AvailableProjects = [];
    }

    [LocalizedDisplayName("ModuleModel.Name")]
    public string Name { get; set; }

    [LocalizedDisplayName("ModuleModel.Description")]
    public string? Description { get; set; }

    [LocalizedDisplayName("ModuleModel.Project")]
    public int? ProjectId { get; set; }

    [LocalizedDisplayName("ModuleModel.Active")]
    public bool Active { get; set; } = true;

    public IList<SelectListItem> AvailableProjects { get; set; }
}
