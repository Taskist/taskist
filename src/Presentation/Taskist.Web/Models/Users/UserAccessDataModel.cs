namespace Taskist.Web.Models.Users;

public class UserAccessDataModel
{
    public int Id { get; set; }

    public string UserName { get; set; }

    public List<int> RoleIds { get; set; } = [];

    public List<string> RoleNames { get; set; } = [];

    public HashSet<int> PermissionIds { get; set; } = [];
}
