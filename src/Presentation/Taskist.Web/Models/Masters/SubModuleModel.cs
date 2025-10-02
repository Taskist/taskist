using Microsoft.AspNetCore.Mvc.Rendering;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Models.Common;

namespace Taskist.Web.Models.Masters;

public class SubModuleModel : BaseModel
{
    public SubModuleModel()
    {
        AvailableModules = [new SelectListItem { Text = "Select", Value = "" }];
        AvailableOwners = [new SelectListItem { Text = "Select", Value = "" }];
    }

    [LocalizedDisplayName("SubModuleModel.Name")]
    public string Name { get; set; }

    [LocalizedDisplayName("SubModuleModel.Description")]
    public string? Description { get; set; }

    [LocalizedDisplayName("SubModuleModel.Module")]
    public int ModuleId { get; set; }

    [LocalizedDisplayName("SubModuleModel.Owner")]
    public int? OwnerId { get; set; }

    public string ModuleName { get; set; }

    [LocalizedDisplayName("SubModuleModel.Active")]
    public bool Active { get; set; } = true;

    public IList<SelectListItem> AvailableModules { get; set; }

    public IList<SelectListItem> AvailableOwners { get; set; }
}
