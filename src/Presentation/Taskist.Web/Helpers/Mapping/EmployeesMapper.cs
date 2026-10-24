using Riok.Mapperly.Abstractions;
using Taskist.Core.Domain.Users;
using Taskist.Core.Domain.Users;
using Taskist.Web.Models.Users;

namespace Taskist.Web.Helpers.Mapping;

[Mapper]
public partial class UsersMapper
{
    public partial UserModel ToModel(User source);

    [MapperIgnoreTarget(nameof(User.Code))]
    [MapperIgnoreTarget(nameof(User.Locked))]
    public partial User ToEntity(UserModel source);

    [MapperIgnoreTarget(nameof(User.Code))]
    [MapperIgnoreTarget(nameof(User.Locked))]
    public partial void Update(UserModel source, User target);

    public partial UserRoleModel ToModel(UserRole source);
    public partial UserRole ToEntity(UserRoleModel source);
    public partial void Update(UserRoleModel source, UserRole target);

    public partial UserRolePermissionModel ToModel(UserRolePermission source);

    public partial UserRolePermissionMapModel ToModel(UserRolePermissionMap source);

    public partial UserProjectModel ToModel(UserProjectMap source);

    [MapperIgnoreTarget(nameof(UserProjectMap.User))]
    [MapperIgnoreTarget(nameof(UserProjectMap.Project))]
    public partial void Update(UserProjectModel source, UserProjectMap target);
}
