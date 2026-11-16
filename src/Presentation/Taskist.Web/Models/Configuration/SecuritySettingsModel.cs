namespace Taskist.Web.Models.Configuration;

public class SecuritySettingsModel
{
    public int PasswordMinLength { get; set; }

    public int MaxFailedLoginAttempts { get; set; }

    public int SessionTimeoutMinutes { get; set; }

    public int PasswordExpiryDays { get; set; }

    public int TokenExpiryMinutes { get; set; }

    public int RememberMeDurationDays { get; set; }
}
