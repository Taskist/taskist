using Taskist.Core.Domain.Common;

namespace Taskist.Core.Domain.Settings;

public class SecuritySettings : ISettings
{
    public int PasswordMinLength { get; set; } = 8;

    public int MaxFailedLoginAttempts { get; set; } = 5;

    public int SessionTimeoutMinutes { get; set; } = 180;

    public int PasswordExpiryDays { get; set; } = 0;

    public int TokenExpiryMinutes { get; set; } = 60;

    public int RememberMeDurationDays { get; set; } = 30;
}
