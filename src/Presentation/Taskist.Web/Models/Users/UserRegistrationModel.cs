using Microsoft.AspNetCore.Mvc.Rendering;

namespace Taskist.Web.Models.Users;

public class UserRegistrationModel
{
    public UserRegistrationModel()
    {
        User = new UserModel();
        AvailableLanguages = [new SelectListItem { Text = "Select", Value = "" }];
        AvailableRoles = [new SelectListItem { Text = "Select", Value = "" }];
    }

    public UserModel User { get; set; }

    public List<SelectListItem> AvailableLanguages { get; set; }

    public List<SelectListItem> AvailableRoles { get; set; }
}
