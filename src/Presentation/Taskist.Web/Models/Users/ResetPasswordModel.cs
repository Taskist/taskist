using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Models.Common;
using System.ComponentModel.DataAnnotations;

namespace Taskist.Web.Models.Users;

public class ResetPasswordModel : BaseModel
{
    [LocalizedDisplayName("ResetPasswordModel.Password")]
    [DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
    public string Password { get; set; }
}
