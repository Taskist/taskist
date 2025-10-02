using System.ComponentModel.DataAnnotations;
using Taskist.Core.Common;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Models.Common;

namespace Taskist.Web.Models.Users;

public class UserModel : BaseModel
{
    public UserModel()
    {
        SelectedRoleIds = [];
        Status = (int)UserStatusEnum.Active;
        EmailWelcomeKit = true;
    }

    [LocalizedDisplayName("UserModel.UserNumber")]
    public string UserNumber { get; set; }

    [LocalizedDisplayName("UserModel.FirstName")]
    public string FirstName { get; set; }

    [LocalizedDisplayName("UserModel.LastName")]
    public string LastName { get; set; }

    [LocalizedDisplayName("UserModel.Email")]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }

    [LocalizedDisplayName("UserModel.Gender")]
    public int GenderId { get; set; }

    [LocalizedDisplayName("UserModel.Language")]
    public int LanguageId { get; set; }

    [LocalizedDisplayName("UserModel.Role")]
    public List<int> SelectedRoleIds { get; set; }

    [LocalizedDisplayName("UserModel.Status")]
    public int Status { get; set; }

    public string Name { get; set; }

    [LocalizedDisplayName("UserModel.EmailWelcomeKit")]
    public bool EmailWelcomeKit { get; set; } = true;
}
