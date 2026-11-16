namespace Taskist.Web.Models.Users;

public class UserAccessPermissionGroupModel
{
    public string GroupName { get; set; }

    public List<string> Permissions { get; set; } = [];
}
