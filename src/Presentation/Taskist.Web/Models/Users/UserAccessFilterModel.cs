using Microsoft.AspNetCore.Mvc.Rendering;

namespace Taskist.Web.Models.Users;

public class UserAccessFilterModel
{
    public IList<SelectListItem> Roles { get; set; } = [];

    public IList<SelectListItem> Permissions { get; set; } = [];
}
