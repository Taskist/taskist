using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Models.Common;

namespace Taskist.Web.Models.Users;

public class UserRoleModel : BaseModel
{
    [LocalizedDisplayName("UserRoleModel.Name")]
    public string Name { get; set; }

    [LocalizedDisplayName("UserRoleModel.SystemName")]
    public string SystemName { get; set; }

    [LocalizedDisplayName("UserRoleModel.Description")]
    public string Description { get; set; }

    [LocalizedDisplayName("UserRoleModel.SystemDefined")]
    public bool SystemDefined { get; set; }

    [LocalizedDisplayName("UserRoleModel.Active")]
    public bool Active { get; set; }
}
