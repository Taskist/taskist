namespace Taskist.Web.Models.Users;

public class UserAccessRowModel
{
    public int Id { get; set; }

    public string UserName { get; set; }

    public string Roles { get; set; }

    public int PermissionCount { get; set; }
}
