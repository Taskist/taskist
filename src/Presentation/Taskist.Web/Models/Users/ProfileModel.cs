using Microsoft.AspNetCore.Mvc.Rendering;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Models.Common;

namespace Taskist.Web.Models.Users;

public class ProfileModel
{
    public ProfileModel()
    {
        Detail = new ProfileDetailModel();
        Password = new ChangePasswordModel();
    }

    public ProfileDetailModel Detail { get; set; }

    public ChangePasswordModel Password { get; set; }

    public int AvatarVersion { get; set; }

    public string DisplayName { get; set; }
}

public class ProfileDetailModel : BaseModel
{
    public ProfileDetailModel()
    {
        AvailableGenders = [
            new SelectListItem { Value = "1", Text = "Male" },
            new SelectListItem { Value = "2", Text = "Female" }
        ];

        AvailableLanguages = [];
    }

    [LocalizedDisplayName("ProfileModel.FirstName")]
    public string FirstName { get; set; }

    [LocalizedDisplayName("ProfileModel.LastName")]
    public string LastName { get; set; }

    [LocalizedDisplayName("ProfileModel.Email")]
    public string Email { get; set; }

    [LocalizedDisplayName("ProfileModel.Gender")]
    public int GenderId { get; set; }

    [LocalizedDisplayName("ProfileModel.Language")]
    public int LanguageId { get; set; }

    public IList<SelectListItem> AvailableGenders { get; set; }

    public IList<SelectListItem> AvailableLanguages { get; set; }
}