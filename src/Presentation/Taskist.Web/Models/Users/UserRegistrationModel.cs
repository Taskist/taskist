using Taskist.Web.Models.Masters;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Taskist.Web.Models.Users;

public class UserRegistrationModel
{
    public UserRegistrationModel()
    {
        User = new UserModel();
        Address = new AddressModel();
        AvailableLanguages = [new SelectListItem { Text = "Select", Value = "" }];
        AvailableRoles = [new SelectListItem { Text = "Select", Value = "" }];
    }

    public UserModel User { get; set; }

    public AddressModel Address { get; set; }

    public List<SelectListItem> AvailableLanguages { get; set; }

    public List<SelectListItem> AvailableRoles { get; set; }
}
