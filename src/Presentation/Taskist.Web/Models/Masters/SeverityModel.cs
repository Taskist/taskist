using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Models.Common;

namespace Taskist.Web.Models.Masters;

public class SeverityModel : BaseModel
{
    [LocalizedDisplayName("SeverityModel.Name")]
    public string Name { get; set; }

    [LocalizedDisplayName("SeverityModel.Description")]
    public string? Description { get; set; }

    [LocalizedDisplayName("SeverityModel.Group")]
    public int GroupId { get; set; }

    public string GroupName { get; set; }

    [LocalizedDisplayName("SeverityModel.TextColor")]
    public string? TextColor { get; set; }

    [LocalizedDisplayName("SeverityModel.BackgroundColor")]
    public string? BackgroundColor { get; set; }

    [LocalizedDisplayName("SeverityModel.IconClass")]
    public string? IconClass { get; set; }

    [LocalizedDisplayName("SeverityModel.Active")]
    public bool Active { get; set; } = true;
}
