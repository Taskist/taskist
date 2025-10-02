using System.ComponentModel.DataAnnotations;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Models.Common;

namespace Taskist.Web.Models.Users;

public class ResetPasswordModel : BaseModel
{
    [LocalizedDisplayName("ResetPasswordModel.Password")]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}
