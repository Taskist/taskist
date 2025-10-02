using Taskist.Core.Domain.Common;

namespace Taskist.Core.Domain.Users;

public class UserRolePermission : BaseEntity
{
    private ICollection<UserRolePermissionMap> _userRolePermissionMaps;

    public string Name { get; set; }

    public string SystemName { get; set; }

    public string RoleGroup { get; set; }

    public bool SystemDefined { get; set; }

    public virtual ICollection<UserRolePermissionMap> UserRolePermissionMaps
    {
        get => _userRolePermissionMaps ??= [];
        protected set => _userRolePermissionMaps = value;
    }
}