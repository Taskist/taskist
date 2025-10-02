using Taskist.Core.Domain.Common;

namespace Taskist.Core.Domain.Users;

public class UserRolePermissionMap : BaseEntity
{
    public int PermissionId { get; set; }

    public int UserRoleId { get; set; }

    public virtual UserRole UserRole { get; set; }

    public virtual UserRolePermission Permission { get; set; }
}