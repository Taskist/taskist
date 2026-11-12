using System.ComponentModel.DataAnnotations;

namespace Taskist.Web.Models.Users;

public class LoginModel
{
    [Display(Name = "Email")]
    public string Email { get; set; }

    [Display(Name = "Password")]
    public string Password { get; set; }

    [Display(Name = "Remember Me")]
    public bool RememberMe { get; set; } = true;

    public bool LocalLogin { get; set; }

    public bool SsoEnabled { get; set; }

    public string? SsoDisplayName { get; set; }

    public bool ShowPasswordForm { get; set; } = true;

    public string? SsoErrorCode { get; set; }

    public bool TwoFactorExpired { get; set; }
}
