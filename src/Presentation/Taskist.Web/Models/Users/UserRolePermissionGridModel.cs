namespace Taskist.Web.Models.Users;

public class UserRolePermissionGridModel
{
    public UserRolePermissionGridModel()
    {
        UserRolePermission = [];
        UserRolePermissionMap = [];
    }

    public int RoleId { get; set; }

    public bool SystemRole { get; set; }

    public bool IsAdmin { get; set; }

    public IList<UserRolePermissionModel> UserRolePermission { get; set; }

    public IList<UserRolePermissionMapModel> UserRolePermissionMap { get; set; }
}