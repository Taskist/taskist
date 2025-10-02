using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Models.Common;

namespace Taskist.Web.Models.Masters;

public class TaskTypeModel : BaseModel
{
    [LocalizedDisplayName("TaskTypeModel.Name")]
    public string Name { get; set; }

    [LocalizedDisplayName("TaskTypeModel.Description")]
    public string? Description { get; set; }

    [LocalizedDisplayName("TaskTypeModel.Group")]
    public int GroupId { get; set; }

    public string GroupName { get; set; }

    [LocalizedDisplayName("TaskTypeModel.TextColor")]
    public string? TextColor { get; set; }

    [LocalizedDisplayName("TaskTypeModel.BackgroundColor")]
    public string? BackgroundColor { get; set; }

    [LocalizedDisplayName("TaskTypeModel.IconClass")]
    public string? IconClass { get; set; }

    [LocalizedDisplayName("TaskTypeModel.Active")]
    public bool Active { get; set; } = true;
}
