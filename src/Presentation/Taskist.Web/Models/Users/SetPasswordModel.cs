using System;
using System.ComponentModel.DataAnnotations;

namespace Taskist.Web.Models.Users;

public class SetPasswordModel
{
    public string Token { get; set; }

    public Guid Code { get; set; }

    [Display(Name = "Name")]
    public string Name { get; set; }

    [Display(Name = "Email")]
    public string Email { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    [Required(ErrorMessage = "Enter the password")]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Required(ErrorMessage = "Retype the password")]
    [Compare("Password", ErrorMessage = "Entered password dosen't match")]
    public string ConfirmPassword { get; set; }

    public bool Valid { get; set; }

    public string Message { get; set; }
}