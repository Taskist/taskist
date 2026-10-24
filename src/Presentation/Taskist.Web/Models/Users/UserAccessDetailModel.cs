namespace Taskist.Web.Models.Users;

public class UserAccessDetailModel
{
    public string UserName { get; set; }

    public List<string> Roles { get; set; } = [];

    public List<UserAccessPermissionGroupModel> PermissionGroups { get; set; } = [];
}
