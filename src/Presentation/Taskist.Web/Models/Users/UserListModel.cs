using Microsoft.AspNetCore.Mvc.Rendering;

namespace Taskist.Web.Models.Users;

public class UserListModel
{
    public IList<SelectListItem> AvailableStatuses { get; set; } = [];

    public IList<SelectListItem> AvailableRoles { get; set; } = [];
}
