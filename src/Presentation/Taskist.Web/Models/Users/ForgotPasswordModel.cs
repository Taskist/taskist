using System.ComponentModel.DataAnnotations;

namespace Taskist.Web.Models.Users;

public class ForgotPasswordModel
{
    [Display(Name = "Email")]
    [Required(ErrorMessage = "You can't leave this blank!")]
    [DataType(DataType.EmailAddress, ErrorMessage = "Provide a valid email")]
    public string Email { get; set; }
}
