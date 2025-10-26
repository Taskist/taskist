using System.ComponentModel.DataAnnotations;
using Taskist.Web.Helpers.Attributes;

namespace Taskist.Web.Models.Users;

public class ChangePasswordModel
{
    [LocalizedDisplayName("ChangePasswordModel.CurrentPassword")]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; }

    [LocalizedDisplayName("ChangePasswordModel.NewPassword")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; }

    [LocalizedDisplayName("ChangePasswordModel.ConfirmNewPassword")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; }
}
