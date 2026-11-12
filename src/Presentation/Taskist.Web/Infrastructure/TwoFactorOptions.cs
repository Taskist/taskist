namespace Taskist.Web.Infrastructure;

public class TwoFactorOptions
{
    #region Fields

    public const string SectionName = "Authentication:TwoFactor";

    public const string PendingScheme = "Taskist.TwoFactor";

    public const string RememberDeviceCookie = "Taskist.TwoFactorDevice";

    public const int MaxAttempts = 5;

    public const int MaxRememberDeviceDays = 365;

    #endregion

    #region Properties

    public bool RequireForAdministrators { get; set; }

    public int RememberDeviceDays { get; set; } = 30;

    public int EffectiveRememberDeviceDays => Math.Clamp(RememberDeviceDays, 0, MaxRememberDeviceDays);

    public bool CanRememberDevice => EffectiveRememberDeviceDays > 0;

    #endregion
}
