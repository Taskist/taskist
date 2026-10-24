namespace Taskist.Core.Domain.Users;

public class DefaultRolePermission
{
    public string UserRoleSystemName { get; set; }

    public IEnumerable<UserRolePermission> PermissionRecords { get; set; }

    public DefaultRolePermission() => PermissionRecords = [];
}