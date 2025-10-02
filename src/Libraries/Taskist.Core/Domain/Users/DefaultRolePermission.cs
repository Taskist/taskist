namespace Taskist.Core.Domain.Users;

public class DefaultRolePermission
{
    public string EmployeeRoleSystemName { get; set; }

    public IEnumerable<UserRolePermission> PermissionRecords { get; set; }

    public DefaultRolePermission()
    {
        PermissionRecords = new List<UserRolePermission>();
    }
}