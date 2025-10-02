using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Models.Common;

namespace Taskist.Web.Models.Masters;

public class StatusModel : BaseModel
{
    [LocalizedDisplayName("StatusModel.Name")]
    public string Name { get; set; }

    [LocalizedDisplayName("StatusModel.Description")]
    public string? Description { get; set; }

    [LocalizedDisplayName("StatusModel.Group")]
    public int GroupId { get; set; }

    public string GroupName { get; set; }

    [LocalizedDisplayName("StatusModel.TextColor")]
    public string? TextColor { get; set; }

    [LocalizedDisplayName("StatusModel.BackgroundColor")]
    public string? BackgroundColor { get; set; }

    [LocalizedDisplayName("StatusModel.IconClass")]
    public string? IconClass { get; set; }

    [LocalizedDisplayName("StatusModel.Active")]
    public bool Active { get; set; } = true;

    public bool SystemDefined { get; set; }
}
