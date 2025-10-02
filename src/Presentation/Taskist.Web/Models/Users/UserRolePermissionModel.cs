using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Models.Common;

namespace Taskist.Web.Models.Users;

public class UserRolePermissionModel : BaseModel
{
    [LocalizedDisplayName("UserRolePermissionModel.Name")]
    public string Name { get; set; }

    [LocalizedDisplayName("UserRolePermissionModel.SystemName")]
    public string SystemName { get; set; }

    [LocalizedDisplayName("UserRolePermissionModel.RoleGroup")]
    public string RoleGroup { get; set; }

    [LocalizedDisplayName("UserRolePermissionModel.SystemPermission")]
    public bool SystemPermission { get; set; }
}