using Taskist.Core.Domain.Common;

namespace Taskist.Core.Domain.Users;

public class UserRole : BaseEntity
{
    private ICollection<UserRolePermissionMap> _userRolePermissionMaps;

    public string Name { get; set; }

    public string SystemName { get; set; }

    public string? Description { get; set; }

    public bool SystemDefined { get; set; }

    public bool Active { get; set; }

    public virtual ICollection<UserRolePermissionMap> UserRolePermissionMaps
    {
        get => _userRolePermissionMaps ??= [];
        protected set => _userRolePermissionMaps = value;
    }
}